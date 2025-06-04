#!/bin/bash
set -e

# Carregar variáveis
source .env

# Verificar pré-requisitos
declare -a REQUIRED_COMMANDS=("terraform" "aws" "ssh")
for cmd in "${REQUIRED_COMMANDS[@]}"; do
  if ! command -v $cmd &> /dev/null; then
    echo "Erro: $cmd não está instalado!"
    exit 1
  fi
done

# Inicializar e aplicar
terraform init
terraform validate
terraform apply -auto-approve \
  -var="region=$AWS_REGION" \
  -var="key_name=$SSH_KEY_NAME" \
  -var-file="terraform.tfvars"


# Mostrar outputs
echo "✅ Infraestrutura criada!"
terraform output