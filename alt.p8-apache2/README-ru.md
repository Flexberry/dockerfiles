# Описание

Данный образ поддерживает функционал `apache2 (версия 4.6.2.7)` сервера 
и является базовым образом для создания специализированных образов запуска `apache2` приложений. 

## Переменные среды

- `MODULES` - список инициализируемых apache-модулей.

Если переменная `MODULES` не определена подключаются модули:
`access_compat`, `alias authz_core`, `authz_host`, `autoindex`, `deflate`, `dir`, `filter`, `include`, `log_config`, `logio`, `mime`, `negotiation`, `rewrite`, `ssl`.


Список доступных с инциализации модулей:
`actions`, `allowmethods`, `asis`, `auth_basic`, `auth_digest`,`auth_form`, `authn_anon`, `authn_core`, `authn_dbd`, `authn_dbm`, `authn_file`, `authn_socache`, `authz_dbd`, `authz_dbm`, `authz_groupfile`, `authz_owner`, `authz_user`, `buffer`, `cache_disk`, `cache`, `cache_socache`, `cgid`, `cgi`, `charset_lite`, `data` , `dav_fs`, `dav` ,`dav_lock`, `dbd`, `dialup`, `dumpio`, `echo`, `env`, `expires`, `ext_filter`, `file_cache`, `headers`, `heartbeat`, `heartmonitor`, `info`,  `lbmethod_bybusyness`, `lbmethod_byrequests`, `lbmethod_bytraffic`, `lbmethod_heartbeat`, `log_debug`, `log_forensic`, `macro`, `mime_magic`, `proxy_ajp`, `proxy_balancer`, `proxy_connect`, `proxy_express`, `proxy_fcgi`, `proxy_fdpass`, `proxy_ftp`, `proxy_hcheck`, `proxy_http`, `proxy`, `proxy_scgi`, `proxy_uwsgi`, `proxy_wstunnel`, `ratelimit`, `reflector`, `remoteip`, `reqtimeout`, `request`, `sed`, `session_cookie`, `session_dbd`, `session`, `setenvif`, `slotmem_plain`, `slotmem_shm`, `socache_dbm`, `socache_memcache`, `socache_shmcb`, `speling`, `status`, `substitute`, `suexec`, `unique_id`, `userdir`, `usertrack`, `version`, `vhost_alias`, `watchdog`.

