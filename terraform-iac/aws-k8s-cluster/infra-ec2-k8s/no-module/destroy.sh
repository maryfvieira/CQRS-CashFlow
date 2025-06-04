#!/bin/bash

echo "🔄 Destruindo recursos Terraform..."
terraform init -upgrade
terraform destroy -auto-approve
echo "✅ Recursos destruídos com sucesso!"
