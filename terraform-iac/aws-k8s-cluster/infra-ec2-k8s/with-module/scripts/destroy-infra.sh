#!/bin/bash

set -ex

read -p "⚠️ Tem certeza que deseja destruir todos os recursos? [s/N]: " confirm
if [[ ! "$confirm" =~ ^[sS]$ ]]; then
  echo "Operação cancelada."
  exit 1
fi

ENV_FILE=".env"

if [ ! -f "$ENV_FILE" ]; then
  echo "❌ Arquivo .env não encontrado. Crie um antes de destruir a infraestrutura."
  exit 1
fi

echo "🔄 Carregando variáveis de ambiente do .env..."
export $(grep -v '^#' $ENV_FILE | xargs)

cd /Users/mary/sources/dotnet/cashflow-cqrs/terraform-iac/aws-k8s-cluster/infra-ec2-k8s/with-module

echo "🚨 Destruindo infraestrutura com Terraform..."

terraform init
terraform destroy -var-file=terraform.tfvars -auto-approve

echo "✅ Infraestrutura destruída com sucesso."
