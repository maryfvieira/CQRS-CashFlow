
# Secrets Manager Resources
resource "aws_secretsmanager_secret" "secrets" {
  for_each = toset([
    "MySql.ConnectionString.Pwd",
    "MongoDb.ConnectionString.Pwd",
    "RabbitMq.Pwd",
    "Jwt.Key"
  ])

  name                    = each.key
  description             = "Secret para ${each.key}"
  recovery_window_in_days = 0 # Para ambientes de teste (em produção use >=7)
}

# Secret Versions (valores sensíveis)
resource "aws_secretsmanager_secret_version" "values" {
  for_each = aws_secretsmanager_secret.secrets

  secret_id     = each.value.id
  secret_string = jsonencode({
    "value" = var.secrets_values[each.key]
  })
}

# External Secrets (Kubernetes)
resource "kubernetes_manifest" "external_secrets" {
  for_each = aws_secretsmanager_secret.secrets

  manifest = {
    apiVersion = "external-secrets.io/v1beta1"
    kind       = "ExternalSecret"
    metadata = {
      name      = lower(replace(each.key, ".", "-"))
      namespace = var.namespace
    }
    spec = {
      refreshInterval = "1h"
      secretStoreRef = {
        name = "aws-secretstore"
        kind = "SecretStore"
      }
      target = {
        name = lower(replace(each.key, ".", "-"))
      }
      data = [
        {
          secretKey = "value"
          remoteRef = {
            key      = each.key
            property = "value"
          }
        }
      ]
    }
  }
}