#!/bin/bash
set -e

cd "$(dirname "$0")/.."

source .env

if [ -z "$AWS_REGION" ] || [ -z "$SSH_KEY_NAME" ]; then
  echo "Erro: variáveis AWS_REGION ou SSH_KEY_NAME não definidas no .env"
  exit 1
fi

declare -a REQUIRED_COMMANDS=("terraform" "aws" "ssh")
for cmd in "${REQUIRED_COMMANDS[@]}"; do
  if ! command -v $cmd &> /dev/null; then
    echo "Erro: $cmd não está instalado!"
    exit 1
  fi
done

terraform init
terraform validate
terraform apply -auto-approve \
  -var="region=$AWS_REGION" \
  -var="key_name=$SSH_KEY_NAME"

echo "✅ Infraestrutura criada!"
terraform output
