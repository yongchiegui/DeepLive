terraform {
  required_version = ">= 0.15.0"  

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.27"
    }
  }
}

provider "aws" {
  region = var.region
}

variable "region" {
  type = string
  default = "ca-central-1"
}

variable "account" {
  type = string
  default = ""
}