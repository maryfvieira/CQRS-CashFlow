#!/bin/bash
set -e

# ------------------------------------------
# Configurações iniciais
# ------------------------------------------
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Carregar variáveis de ambiente
if [ -f .env ]; then
  source .env
else
  echo -e "${RED}Arquivo .env não encontrado!${NC}"
  exit 1
fi

# ------------------------------------------
# 1. Verificar pré-requisitos
# ------------------------------------------
echo -e "${GREEN}Verificando pré-requisitos...${NC}"

check_command() {
  if ! command -v $1 &> /dev/null; then
    echo -e "${RED}Erro: $1 não está instalado!${NC}"
    exit 1
  fi
}

check_command "terraform"
check_command "aws"
check_command "kubectl"
check_command "helm"

# ------------------------------------------
# 2. Inicializar e aplicar o Terraform
# ------------------------------------------
echo -e "${GREEN}Iniciando provisionamento com Terraform...${NC}"

terraform init
terraform validate

terraform apply -auto-approve \
  -var="aws_region=$AWS_REGION" \
  -var="cluster_name=$CLUSTER_NAME" \
  -var="aws_account_id=$AWS_ACCOUNT_ID" \
  -var="namespace=$NAMESPACE" \
  -var="secrets_values=$SECRETS_VALUES"

# ------------------------------------------
# 3. Verificar criação dos recursos
# ------------------------------------------
echo -e "${GREEN}Verificando recursos na AWS...${NC}"

# Listar secrets criados
aws secretsmanager list-secrets --region $AWS_REGION \
  --query "SecretList[?Name=='MySql.ConnectionString.Pwd' || Name=='MongoDb.ConnectionString.Pwd' || Name=='RabbitMq.Pwd' || Name=='Jwt.Key']" \
  --output table

echo -e "${GREEN}Verificando recursos no Kubernetes...${NC}"

# Verificar ESO
kubectl get pods -n external-secrets

# Verificar ExternalSecrets
kubectl get externalsecrets -n $NAMESPACE

# Verificar Secrets
kubectl get secrets -n $NAMESPACE

# ------------------------------------------
# 4. Output final
# ------------------------------------------
echo -e "${GREEN}✅ Provisionamento completo! Recursos criados:${NC}"
echo "- Secrets Manager: 4 secrets"
echo "- IAM Role/Policies: 1 role + 1 policy"
echo "- ESO: 1 deployment"
echo "- SecretStore: 1"
echo "- ExternalSecrets: 4"