#!/bin/bash

set -e

ENV_FILE=".env"

if [ ! -f "$ENV_FILE" ]; then
  echo "❌ Arquivo .env não encontrado. Crie um antes de aplicar a infraestrutura."
  exit 1
fi

echo "🔄 Carregando variáveis de ambiente do .env..."
export $(grep -v '^#' $ENV_FILE | xargs)

echo "🚀 Iniciando aplicação da infraestrutura Kubernetes..."

cd /Users/mary/sources/dotnet/cashflow-cqrs/terraform-iac/aws-k8s-cluster/infra-ec2-k8s/with-module

echo "📦 Executando Terraform Init..."
terraform init
terraform validate

echo "⚙️ Aplicando Terraform com tfvars e variáveis de ambiente..."
terraform apply -var-file=terraform.tfvars -auto-approve

echo "✅ Infraestrutura provisionada com sucesso!"

