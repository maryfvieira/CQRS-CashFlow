#!/bin/bash
set -e

# ------------------------------------------
# 1. Destruir recursos Terraform
# ------------------------------------------
echo "Iniciando destruição dos recursos Terraform..."
terraform destroy -auto-approve

# ------------------------------------------
# 2. Remover Helm Release do ESO
# ------------------------------------------
echo "Removendo External Secrets Operator..."
helm uninstall external-secrets -n external-secrets || echo "Helm release já removido"

# ------------------------------------------
# 3. Deletar secrets manualmente (fallback)
# ------------------------------------------
SECRETS=(
  "MySql.ConnectionString.Pwd"
  "MongoDb.ConnectionString.Pwd"
  "RabbitMq.Pwd"
  "Jwt.Key"
)

REGION="sa-east-1"

for SECRET in "${SECRETS[@]}"; do
  echo "Removendo secret: $SECRET"
  aws secretsmanager delete-secret \
    --secret-id "$SECRET" \
    --force-delete-without-recovery \
    --region "$REGION" || echo "Secret não encontrado, continuando..."
done

# ------------------------------------------
# 4. Limpar recursos do Kubernetes
# ------------------------------------------
echo "Limpando recursos do Kubernetes..."
kubectl delete externalsecrets --all -A --ignore-not-found
kubectl delete secretstore --all -A --ignore-not-found
kubectl delete ns external-secrets --ignore-not-found

# ------------------------------------------
# 5. Remover estado local do Terraform
# ------------------------------------------
echo "Limpando estado local..."
rm -rf .terraform .terraform.lock.hcl terraform.tfstate*

echo "✅ Desconstrução completa!"