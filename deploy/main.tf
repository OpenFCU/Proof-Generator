terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
}

provider "aws" {
  region     = "ap-northeast-1"
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

# API Gateway v2

resource "aws_apigatewayv2_api" "proof_generator" {
  name          = "proof_generator"
  protocol_type = "HTTP"
  cors_configuration {
    allow_credentials = false
    allow_headers     = []
    allow_methods = [
      "POST",
    ]
    allow_origins = [
      "https://*",
    ]
    expose_headers = []
    max_age        = 3600
  }
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.proof_generator.id
  name        = "$default"
  auto_deploy = true

  access_log_settings {
    destination_arn = "arn:aws:logs:ap-northeast-1:143348218800:log-group:http_api_log"
    format          = "$context.identity.sourceIp - - [$context.requestTime] \"$context.httpMethod $context.routeKey $context.protocol\" $context.status $context.responseLength $context.requestId | $context.integrationErrorMessage"
  }
}

resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id                 = aws_apigatewayv2_api.proof_generator.id
  integration_type       = "AWS_PROXY"
  connection_type        = "INTERNET"
  payload_format_version = "2.0"
  integration_uri        = aws_lambda_function.proof_generator.invoke_arn
}

resource "aws_apigatewayv2_route" "proof" {
  api_id    = aws_apigatewayv2_api.proof_generator.id
  route_key = "POST /"

  target = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_lambda_permission" "api_gateway" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.proof_generator.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.proof_generator.execution_arn}/*/*"
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
  image_uri     = "${aws_ecr_repository.proof_generator.repository_url}@sha256:<SHA256>"
  timeout       = 15
  memory_size   = 256
}
