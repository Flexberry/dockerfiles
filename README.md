# Flexberry ORM ODataService

[![Build Status Master](https://travis-ci.org/Flexberry/NewPlatform.Flexberry.ORM.ODataService.svg?branch=master)](https://travis-ci.org/Flexberry/NewPlatform.Flexberry.ORM.ODataService)

[![Build Status Develop](https://travis-ci.org/Flexberry/NewPlatform.Flexberry.ORM.ODataService.svg?branch=develop)](https://travis-ci.org/Flexberry/NewPlatform.Flexberry.ORM.ODataService)

В этом репозитории располагается исходный код `Flexberry ORM ODataService` - серверного компонета для реализации публикации данных по протоколу [OData V4](http://www.odata.org/) для `Microsoft .NET Framework`.

## Ключевые особенности

* Возможность публикации модели данных `Flexberry ORM` без необходимости доработки или генерации кода - достаточно иметь скомпилированную сборку с объектами данных.
* Широкие возможности по кастомизации, включая возможность управления запросами, передаваемыми в `Flexberry ORM`.
* Поддержка `Mono` (отсутствие неуправляемого кода).

## Использование

Для работы с `Flexberry ORM ODataService` требуется наличие сборки с объектами данных `Flexberry ORM`. OData-сервер работает поверх `WebApi`, поэтому конфигурация выполняется в соответствующем стиле. Подробнее с конфигурацией можно познакомиться в [документации](https://flexberry.github.io/ru/flexberry-o-r-m-o-data-service.html).

## Структура проекта

Данное решение содержит несколько проектов, которые можно условно разделить две категории:

* Реализация OData-сервера
  * `NewPlatform.Flexberry.ORM.ODataService` - основной проект, в котором располагаются классы для публикации с объектов данных по протоколу `OData`.
* Проекты для тестов
  * `NewPlatform.Flexberry.ORM.ODataService.Tests` - проект с интеграционными тестами (для их исполнения требуются различные СУБД).
  * `NewPlatform.Flexberry.ORM.ODataService.Tests(Objects)` - объекты для проекта с тестами
  * `NewPlatform.Flexberry.ORM.ODataService.Tests(BusinessServers)` - бизнес-логика объектов проекта с тестами.

## Тестирование

Реализованы интеграционные тесты. Для выполнения интеграционных тестов требуется наличие СУБД: Microsoft SQL, Postgres, Oracle. Соответствующие строки соединения задаются в конфигурационном файле проекта с интеграционными тестами. При выполнении тестов для каждого тестового метода создаётся временная БД (скрипты есть в проекте с интеграционными тестами). Структура данных для тестов сгенерирована при помощи Flexberry Designer, метаданные выгружены в виде crp-файла.

## Документация

Документация разработчика размещается в разделе `Flexberry ORM` на сайте [https://flexberry.github.io](https://flexberry.github.io/ru/fo_landing_page.html).
Автогенерируемая документация по API размещается в ветке `gh-pages` и доступна пользователям по адресу: [TODO: autodoc URL]()

## Сообщество

Основным способом распространения `Flexberry ORM ODataService` является [NuGet-пакет](https://www.nuget.org/packages/NewPlatform.Flexberry.ORM.ODataService). Если во время использования этого фреймворка вы обнаружили ошибку или проблему, то можно завести Issue или исправить ошибку и отправить в этот репозиторий соответствующий Pool Request.

### Доработка

Исправление ошибок приветствуется, технические детали можно выяснить в [чате](https://gitter.im/Flexberry/PlatformDevelopment) или непосредственно в описании Issue.
Добавление новой функциональности рекомендуется согласовывать с авторами, поскольку принятие Pool Request в этом случае может быть затруднено.

### Техническая поддержка

Авторы оставляют за собой право выполнять доработки и исправление ошибок самостоятельно без каких-либо гарантий по срокам. В случае необходимости получения приоритетной технической поддержки с фиксированными сроками, то условия проведения данной работы можно обговорить в частном порядке по [E-Mail](mailto:mail@flexberry.net).

## Ссылки

* [Информация на официальном сайте](http://flexberry.ru/FlexberryORM)
* [Документация](https://flexberry.github.io/ru/fo_landing_page.html)
* [Лицензия (MIT)](LICENSE.md)
* [Лог изменений](CHANGELOG.md)
* [Установить через NuGet](https://www.nuget.org/packages/NewPlatform.Flexberry.ORM.ODataService)
* [Gitter чат](https://gitter.im/Flexberry/PlatformDevelopment)
* [E-Mail](mailto:mail@flexberry.net)