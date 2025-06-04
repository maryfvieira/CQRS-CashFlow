output "secret_arns" {
  value = {
    for k, v in aws_secretsmanager_secret.secrets : k => v.arn
  }
  description = "ARNs das secrets criadas"
}

output "external_secrets_status" {
  value = helm_release.external_secrets.status
  description = "Status da instalação do ESO"
}