output "master_public_ip" {
  value = aws_instance.this[0].public_ip
}

output "worker_ips" {
  value = slice(aws_instance.this[*].public_ip, 1, length(aws_instance.this[*].public_ip))
}

output "ssh_command" {
  value = "ssh -i ~/.ssh/${var.key_name} ubuntu@${aws_instance.this[0].public_ip}"
}

output "instance_public_dns" {
  value = aws_instance.this[*].public_dns
}