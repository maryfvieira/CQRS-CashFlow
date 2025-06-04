output "master_public_ip" {
  value = module.ec2_instance.master_public_ip
}

output "worker_ips" {
  value = module.ec2_instance.worker_ips
}

output "ssh_command" {
  value = module.ec2_instance.ssh_command
}