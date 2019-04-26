# Установка nginx-сервера от cryptopro под ALTLinux

Общая процедура установки описана в документе:
[Настройка nginx для работы с сертификатами ГОСТ 2012 года](https://www.cryptopro.ru/forum2/default.aspx?g=posts&t=12505)

Для дистрибутивов семейства ALTLinux требуются определенные изменения, описанные в данном документе.

## Установка nginx-сервера без использования docker

### Создание бинарных кодов nginx, cprocsp, openssl-1.1 

Указанная в документе процедура `install-nginx.sh` не работает для дистрибутивов ALTLinux по следующим причинам:
- ALTLinux как и дистрибутивы centos, red hatиспользует пакеты RPM, но командой установки пакетов является команда `apt-get`, а не `yum`;
- для установки компиляторов `gcc`, `gcc-c++` требуется указать конкретную версию (в нашем случае 4.7) компиляторов из доступного набора.

Данный скрипт был доработан и размещен в `git` репозитории под аналогичным именем [install-nginx.sh](https://github.com/Flexberry/dockerfiles/blob/master/nginx-ssl-gost2012/install-nginx.sh).

Перед установкой `nginx` необходимо в каталог, где  размещен скрипт `install-nginx.sh` (здесь и далее каталог `/root/`) поместить требуемую `X64/RPM` версию `КриптоПро CSP/UNIX` со страницы [КриптоПро CSP - Загрузка файлов](https://www.cryptopro.ru/products/csp/downloads).

В нашем случае это сертифицированная версия 
```
КриптоПро CSP 5.0 для Linux (x64, rpm)
Контрольная сумма
ГОСТ: 0FC217AFB78E43F213289DCD16B691A7C0F7CD4AB11B5D711CA3A94818C86C23
MD5: 11fa99df04978b4debb5fcb1c59cd447
```
Скаченный файл имеет имя `linux-amd64.tgz`.

Запуск команды:
```
# apt-get update 
# chmod a+x install-nginx.sh
# ./install-nginx.sh --csp=linux-amd64.tgz
```

Результат выполнения команды:
- промежуточный файлы `*.rpm`, `*.tar.gz`,  и каталоги в текущем каталоге (`/root/`);
- бинарные файлы, библиотеки в каталаге `/opt/cprocsp/`;
- бинарный код `/usr/sbin/nginx` и настройки в каталоге `/etc/nginx`. 


### Генерация и установка сертификата

Добавление в тропу `PATH` каталогов
`/opt/cprocsp/bin/amd64`, `/opt/cprocsp/sbin/amd64`, `/opt/cprocsp/cp-openssl-1.1.0/bin/amd64` и
дополнительных каталогов библиотек
`/usr/local/lib`, `/opt/cprocsp/lib/amd64/`,`/opt/cprocsp/cp-openssl-1.1.0/lib/amd64/`.
в переменную `LD_LIBRARY_PATH`.
```
# .  /root/cryptopro-paths.sh
```
Убедитесь, что переменаая PATH изменилась:
```
# echo $PATH
/opt/cprocsp/bin/amd64:/opt/cprocsp/sbin/amd64:/opt/cprocsp/cp-openssl-1.1.0/bin/amd64:/root/bin:/sbin:/usr/sbin:/usr/local/sbin:/bin:/usr/bin:/usr/local/bin
# echo  LD_LIBRARY_PATH
/usr/local/lib:/opt/cprocsp/lib/amd64/:/opt/cprocsp/cp-openssl-1.1.0/lib/amd64/
```

Генерация ключей:
```
# wget https://raw.githubusercontent.com/fullincome/scripts/master/nginx-gost/install-certs.sh && chmod +x install-certs.sh
# ./install-certs.sh
```
После запроса
```
CryptCP 5.0 (c) "Crypto-Pro", 2002-2018.
Command prompt Utility for file signature and encryption.
Creating request...
Press keys...
```
необходимо ввести на клавиатуре длинную последовательность символов.
Пароль ключа должен быть пустой (введите два раза `Enter` на запросы `New password:`, `Confirm password:`).
На запрос:
```
Do you want to add it to Root store? [Y]es or [N]o:
```
ответьте `Y`.
На запрос 
```
CPCSP: Warning: installing a root certificate with an unconfirmed thumbprint is a security risk. Do you want to install this certificate?
Thumbprint (sha1): 046255290B0EB1CDD1797D9AB8C81F699E3687F3
(o)OK, (c)Cancel
```
ответьте `O`.

### Добавление пользователя nginx

Запустите скрипт `addNginxUser.sh`.
В ходе работы скрипта производится:
- создание группы `nginx`;
- создание пользователя `nginx`;
- установка пользователя `nginx` в файле конфигурации `/etc/nginx/nginx.conf`. 

### Установка стартового скрипта

- Скопируйте стартовый скрипт nginx в каталог `/etc/init.d/`
- Сконфигурируйте его командой:
```
# chkconfig --add nginx
```
- Запустите скрипт:
```
# service nginx start
```

## Установка nginx-сервера с использованием docker

Установка с использованием docker имеет следующее преимущества:
- создание docker-образов для платформ `ALTLinux P7`, `ALTLinux P8`;
- создание TGZ-архивов для установки `nginx` на произвольное количество серверов что позволяет избежать установки ненужного в дальнейшем набора пакетов: `make`, `gcc`, `gcc+`. 

Создание docker-образов обеспечивает скрипт `makeNginxImage.sh`. 
Скрипту передается параметр, определеющий платформу ALTLinux:
- `7` - `ALTLinux C7`, `P7`;
- `8` - `ALTLinux C8`, `P8`.

Скрипт выполняет следущие щаги:
- Создание промежуточного обаза с бинарными кодами `nginx`, `cprocsp`, `openssl-1.1`.
- Запуск образа в виде контейнера для генерации и установка сертификата, генерация TGZ-архива. 
- Копирование  TGZ-архива в OST-систему;
- Создание промежуточного образа.

В результате выполнения скрипта генерируются:
- для `ALTLinux C7`: образ  `kafnevod/nginx-gost2.0:p7`, архив `nginx_p7.tgz`;
- для `ALTLinux C8`: образ  `kafnevod/nginx-gost2.0:p8`, архив `nginx_p8.tgz`.

Вы можете либо использовать полученные docker-образы присвоив им в случае необходимости другие теги, либо установить `nginx` непосредственно в `HOST-систему` распаковав полученные архивы. 

Для установки бинарных кодов из архива поместите архив на целевые машины и выполните команды:
```
cd /
tar xvzf .../nginx_p7.tgz
```
для дистрибутивов `ALTLinux С7`, `ALTLinux P7` и
```
cd /
tar xvzf .../nginx_p8.tgz
```
для дистрибутива `ALTLinux С8`, `ALTLinux P8`

Для установки пользователя nginx выполните скрипт `addNginxUser.sh` и команду:
```
# chkconfig --add nginx
```



