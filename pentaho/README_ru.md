# Flexberry/pentaho

## Семество образов Flexberry/pentaho

Данное семество образов поддерживает сервис аналитики [Pentaho](http://pentaho.org/) для проектов [Flexberry](https://github.com/Flexberry).


Особенности семейства:
- максимальная локализация (русификация);
- встроенная поддержка драйверов SQL (postgres, ...) и NoSQL (ClickHouse. ...) баз данных

На настроящий момент семейство содержит единственный образ  [flexberry/pentaho-saiku](https://github.com/Flexberry/dockerfiles/tree/pentaho-saiku_8.0/pentaho) версии 8.0 с алиасами:
```
flexberry/pentaho = flexberry/pentaho:8.0 = flexberry/pentaho-saiku = flexberry/pentaho-saiku:8.0
```

В настоящий момент начинается разработка дерево образов 8.1.
Его особенности:
- образ flexberry/pentaho:8.1 содержит минимальный набор функционала входящий в ядро Pentaho 8.1 CE
- образ flexberry/pentaho-saiku:8.1 является дочерним образа flexberry/pentaho:8.1. При его сборке будет использоваться исходные коды проектов
[/Flexberry/saiku](https://github.com/Flexberry/saiku),
[Flexberry/fop](https://github.com/Flexberry/fop).


Дерево образов будет выглядеть следующим образом:

```
flexberry/pentaho = flexberry/pentaho:8.0 == flexberry/pentaho-saiku = flexberry/pentaho-saiku:8.0
                  |
                  + flexberry/pentaho:8.1 -> flexberry/pentaho-saiku:8.1
```

По мере готовности алиасы образов flexberry/pentaho, flexberry/pentaho-saiku будут переноситься с ветки 8.0 на ветку 8.1
и последующие версии образов.
