terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
}

provider "aws" {
  region     = var.aws_region
  access_key = var.aws_access_key
  secret_key = var.aws_secret_key
}

# IAM

data "aws_iam_policy_document" "assume_lambda_policy" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "lambda_basic_role" {
  name                = "lambda_basic_role"
  assume_role_policy  = data.aws_iam_policy_document.assume_lambda_policy.json
  managed_policy_arns = ["arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"]
}

# S3

resource "aws_s3_bucket" "proof_generator" {
  bucket = var.s3_bucket_name
}

resource "aws_s3_object" "index_html" {
  bucket       = aws_s3_bucket.proof_generator.id
  key          = "index.html"
  source       = "../static/src/index.html"
  etag         = filemd5("../static/src/index.html")
  content_type = "text/html"
}

resource "aws_s3_object" "style_css" {
  bucket       = aws_s3_bucket.proof_generator.id
  key          = "style.css"
  source       = "../static/src/style.css"
  etag         = filemd5("../static/src/style.css")
  content_type = "text/css"
}

resource "aws_s3_bucket_website_configuration" "website_config" {
  bucket = aws_s3_bucket.proof_generator.id
  index_document {
    suffix = "index.html"
  }
}

data "aws_iam_policy_document" "allow_public_access" {
  statement {
    sid       = "PublicReadGetObject"
    effect    = "Allow"
    actions   = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.proof_generator.arn}/*"]
    principals {
      type        = "*"
      identifiers = ["*"]
    }
  }
}

resource "aws_s3_bucket_policy" "allow_public_access" {
  bucket = aws_s3_bucket.proof_generator.id
  policy = data.aws_iam_policy_document.allow_public_access.json
}

resource "aws_s3_bucket_public_access_block" "public_access_block" {
  bucket                  = aws_s3_bucket.proof_generator.id
  block_public_acls       = false
  block_public_policy     = false
  ignore_public_acls      = false
  restrict_public_buckets = false
}

# API Gateway v2

resource "aws_apigatewayv2_api" "proof_generator" {
  name                         = "proof_generator"
  protocol_type                = "HTTP"
  disable_execute_api_endpoint = true
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.proof_generator.id
  name        = "$default"
  auto_deploy = true

  default_route_settings {
    throttling_rate_limit  = 5
    throttling_burst_limit = 5
  }

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.apigateway_log.arn
    format          = "$context.identity.sourceIp - - [$context.requestTime] \"$context.routeKey $context.protocol\" $context.status $context.responseLength $context.requestId | $context.integrationErrorMessage"
  }
}

resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id                 = aws_apigatewayv2_api.proof_generator.id
  integration_type       = "AWS_PROXY"
  connection_type        = "INTERNET"
  payload_format_version = "2.0"
  integration_uri        = aws_lambda_function.proof_generator.invoke_arn
}

resource "aws_apigatewayv2_route" "cors_preflight" {
  api_id    = aws_apigatewayv2_api.proof_generator.id
  route_key = "OPTIONS /"

  target = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_route" "generate" {
  api_id    = aws_apigatewayv2_api.proof_generator.id
  route_key = "POST /"

  target = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_route" "download" {
  api_id    = aws_apigatewayv2_api.proof_generator.id
  route_key = "GET /generated/{proxy+}"

  target = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_integration" "s3_proxy" {
  api_id             = aws_apigatewayv2_api.proof_generator.id
  integration_type   = "HTTP_PROXY"
  connection_type    = "INTERNET"
  integration_method = "GET"
  integration_uri    = "http://${aws_s3_bucket_website_configuration.website_config.website_endpoint}/{proxy}"
}

resource "aws_apigatewayv2_route" "static_files" {
  api_id    = aws_apigatewayv2_api.proof_generator.id
  route_key = "GET /{proxy+}"

  target = "integrations/${aws_apigatewayv2_integration.s3_proxy.id}"
}

resource "aws_lambda_permission" "apigateway" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.proof_generator.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.proof_generator.execution_arn}/*/*"
}

# CloudWatch

resource "aws_cloudwatch_log_group" "apigateway_log" {
  name              = "apigateway_log"
  retention_in_days = 30
}

# ECR

resource "aws_ecr_repository" "proof_generator" {
  name = "proof-generator"
}

output "aws_ecr_repository_url" {
  value = aws_ecr_repository.proof_generator.repository_url
}

# Lambda

resource "aws_lambda_function" "proof_generator" {
  function_name = "proof_generator"
  role          = aws_iam_role.lambda_basic_role.arn
  package_type  = "Image"
  image_uri     = "${aws_ecr_repository.proof_generator.repository_url}:<tag>"
  timeout       = 15
  memory_size   = 1024
}
