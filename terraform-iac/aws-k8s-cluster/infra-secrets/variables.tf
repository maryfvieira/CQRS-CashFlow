variable "aws_region" {
  description = "Região da AWS"
  type        = string
  default     = "sa-east-1"
}

variable "cluster_name" {
  description = "Nome do cluster EKS"
  type        = string
}

variable "aws_account_id" {
  description = "AWS Account ID"
  type        = string
}

variable "namespace" {
  description = "Namespace para deploy dos recursos"
  type        = string
  default     = "default"
}