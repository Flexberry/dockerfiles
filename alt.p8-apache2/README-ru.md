# Описание

Данный образ поддерживает функционал `apache (версия 4.6.2.7)` сервера 
и является базовым образом для создания специализированных образов запуска `apache2` приложений. 

## Переменные среды

- `MODULES` - список инициализируемых apache-модулей.

Список доступных с инциализации модулей:
`access_compat` ,`actions` ,`alias` ,`allowmethods` ,`asis` ,`auth_basic` ,`auth_digest` ,`auth_form` ,`authn_anon` ,`authn_core`,`authn_dbd` ,`authn_dbm` ,`authn_file` ,`authn_socache` ,`authz_core` ,`authz_dbd` ,`authz_dbm` ,`authz_groupfile` ,`authz_host` ,`authz_owner` ,`authz_user` ,`autoindex` ,`buffer` ,`cache_disk` ,`cache` ,`cache_socache` ,`cgid` ,`cgi` ,`charset_lite` ,`data` ,`dav_fs` ,`dav` ,`dav_lock` ,`dbd` ,`deflate` ,`dialup` ,`dir` ,`dumpio` ,`echo` ,`env` ,`expires` ,`ext_filter` ,`file_cache` ,`filter` ,`headers` ,`heartbeat` ,`heartmonitor` ,`include` ,`info` ,
`lbmethod_bybusyness` ,`lbmethod_byrequests` ,`lbmethod_bytraffic` ,`lbmethod_heartbeat` ,`log_config` ,`log_debug` ,`log_forensic` ,`logio` ,`macro` ,`mime` ,`mime_magic` ,`mono` ,`negotiation` ,`proxy_ajp` ,`proxy_balancer` ,`proxy_connect` ,`proxy_express` ,`proxy_fcgi` ,`proxy_fdpass` ,`proxy_ftp` ,`proxy_hcheck` ,`proxy_http` ,`proxy` ,`proxy_scgi` ,`proxy_uwsgi` ,`proxy_wstunnel` ,`ratelimit` ,`reflector` ,`remoteip` ,`reqtimeout` ,`request` ,`rewrite` ,`sed` ,`session_cookie` ,`session_dbd` ,`session` ,`setenvif` ,`slotmem_plain` ,`slotmem_shm` ,`socache_dbm` ,`socache_memcache` ,`socache_shmcb` ,`speling` ,`ssl` ,`status` ,`substitute` ,`suexec` ,`unique_id` ,`userdir` ,`usertrack` ,`version` ,`vhost_alias` ,`watchdog`

