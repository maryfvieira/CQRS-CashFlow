output "master_public_ip" {
  value = aws_instance.k8s_node[0].public_ip
}

output "worker_ips" {
  value = slice(aws_instance.k8s_node[*].public_ip, 1, length(aws_instance.k8s_node[*].public_ip))
}

output "ssh_command" {
  value = "ssh -i ~/.ssh/${var.key_name} ubuntu@${aws_instance.k8s_node[0].public_ip}"
}

output "kube_join_command" {
  value = "Execute no master: kubeadm token create --print-join-command"
}