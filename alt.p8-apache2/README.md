# Description

This image supports apache2 functionality (version 4.6.2.7) `server
and is a basic way to create specialized images of running `apache2` applications.

## Environment Variables

- `MODULES` - a list of initialized apache-modules.

If the variable `MODULES` is not defined, the modules are connected:
`access_compat`,` alias authz_core`, `authz_host`,` autoindex`, `deflate`,` dir`, `filter`,` include`, `log_config`,` logio`, `mime`,` negotiation`, ` rewrite`, `ssl`.


The list of modules available with module initialization:
`actions`,` allowmethods`, `asis`,` auth_basic`, `auth_digest`,` auth_form`, `authn_anon`,` authn_core`, `authn_dbd`,` authn_dbm`, `authn_file`,` authn_socache`, `authn_dbm`,` authn_file`, `authn_socache`,` authn_dbm`, `authn_file`,` authn_socache`, `authn_dbm`,` authn_file`, `authn_socache`,` authn_dbm`, `authn_file` `,` authz_dbm`, `authz_groupfile`,` authz_owner`, `authz_user`,` buffer`, `cache_disk`,` cache`, `cache_socache`,` cgid`, `cgi`,` charset_lite`, data, `dav_fs`,` dav`, `dav_lock`,` dbd`, `dialup`,` dumpio`, `echo`,` env`, `expires`,` ext_filter`, `file_cache`,` headers`, `heartbeat `,` heartmonitor`, `info`,` lbmethod_bybusyness`, `lbmethod_byrequests`,` lbmethod_bytraffic`,
`lbmethod_heartbeat`,` log_debug`, `log_forensic`,` macro`, `mime_magic`,` proxyj4, jim, jim, jime, jim, jim, log log_forensic` `proxy_connect`,` proxy_express`, `proxy_fcgi`,` proxy_fdpass`, `proxy_ftp`,` proxy_hcheck`, `proxy_http`,` proxy`, `proxy_scgi`,` proxy_uwsgi`, `proxy_wstunnel`,` templim ', `temphim`,` proxy_uwsgi` ,` `,` remoteip`, `reqtimeout`,` request`, `sed`,` session_cookie`, `session_dbd`,` session`, `setenvif`,` slotmem_plain`, `slotmem_shm`,` socache_dbm`, `socach e_memcache`, `socache_shmcb`,` speling`, `status`,` substitute`, `suexec`,` unique_id`, `userdir`,` usertrack`, `version`,` vhost_alias`, `watchdog`.
