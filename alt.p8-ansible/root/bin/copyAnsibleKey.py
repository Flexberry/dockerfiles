#! /usr/bin/env python
import yaml, os, sys

inventory = yaml.safe_load(sys.stdin)
#print(inventory)

hosts = {}
for child in inventory['all']['children']:
  for Host in inventory['all']['children'][child]['hosts']:
    host=Host
    user='icsadmin'
    hostInfo = inventory['all']['children'][child]['hosts'][host]
    if not type(hostInfo) is dict:
      hosts[host] = user
      continue
    if 'ansible_ssh_host' in hostInfo:
      host = hostInfo['ansible_ssh_host']
    if 'ansible_host' in hostInfo:
      host = hostInfo['ansible_host']
    if 'ansible_ssh_user' in hostInfo:
      user = hostInfo['ansible_ssh_user']
    if 'ansible_user' in hostInfo:
      user = hostInfo['ansible_user']
    hosts[host] = user
    #print 'child=', child, 'Host=', Host, 'host=', host,'user=' , user, 'hostInfo=', hostInfo
#print 'HOSTS=', hosts

for host in hosts:
  user = hosts[host]
  cmd = 'ssh-copy-id '+ user + '@' + host
  #print 'CMD=', cmd
  os.system(cmd)
