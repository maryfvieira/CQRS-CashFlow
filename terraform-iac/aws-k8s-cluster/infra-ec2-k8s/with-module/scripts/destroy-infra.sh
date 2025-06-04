#!/bin/bash

set -e

read -p "⚠️ Tem certeza que deseja destruir todos os recursos? [s/N]: " confirm
if [[ ! "$confirm" =~ ^[sS]$ ]]; then
  echo "Operação cancelada."
  exit 1
fi

# Destruir recursos Terraform
terraform destroy -auto-approve

# Limpar arquivos de estado
rm -rf .terraform/ terraform.tfstate terraform.tfstate.backup

echo "✅ Todos os recursos foram destruídos!"