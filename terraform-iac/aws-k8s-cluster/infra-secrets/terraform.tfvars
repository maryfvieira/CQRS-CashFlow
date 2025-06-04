aws_region     = "sa-east-1"
cluster_name   = "meu-cluster-eks"
aws_account_id = "123456789012"
namespace      = "meu-namespace"

secrets_values = {
  "MySql.ConnectionString.Pwd"  = "senhaSuperSecretaMySQL"
  "MongoDb.ConnectionString.Pwd" = "senhaMongoDB123"
  "RabbitMq.Pwd"                = "senhaRabbitMQ@456"
  "Jwt.Key"                     = "bXktc3VwZXItc2VjcmV0LWtleS1sb25nYS1tYWlzLWxvbmdhLXF1ZS1wb2RlLXNlci1nZXJhZGE="
}