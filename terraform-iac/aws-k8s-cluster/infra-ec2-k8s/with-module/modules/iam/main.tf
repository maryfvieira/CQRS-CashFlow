
resource "aws_iam_role" "k8s_ec2_role" {
  name = var.role_name

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Principal = {
          Service = "ec2.amazonaws.com"
        },
        Action = "sts:AssumeRole"
      }
    ]
  })
}

# Política inline para SSM Parameter Store
resource "aws_iam_role_policy" "ssm_access_policy" {
  name = "ssm-access"
  role = aws_iam_role.k8s_ec2_role.id

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "ssm:GetParameter",
          "ssm:PutParameter"
        ],
        Resource = "*"
      }
    ]
  })
}

# (Opcional) Acesso ao Amazon ECR público/privado
resource "aws_iam_role_policy_attachment" "ecr" {
  role       = aws_iam_role.k8s_ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
}

# (Opcional) Acesso ao EBS CSI Driver
resource "aws_iam_role_policy_attachment" "ebs" {
  role       = aws_iam_role.k8s_ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy"
}

# Profile que será usado nas instâncias EC2
resource "aws_iam_instance_profile" "ec2_profile" {
  name = "${var.role_name}-profile"
  role = aws_iam_role.k8s_ec2_role.name
}
