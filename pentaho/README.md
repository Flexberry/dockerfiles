# Flexberry / pentaho

## Family images of Flexberry/pentaho

This family of images supports the [Pentaho]((http://pentaho.org/)) analytics service for projects [Flexberry] (https://github.com/Flexberry).


Features of the family:
- maximum localization (Russification);
- built-in support for SQL drivers (postgres, ...) and NoSQL (ClickHouse. ...) databases

At the present moment, the family contains a single image [flexberry / pentaho-saiku] (https://github.com/Flexberry/dockerfiles/tree/pentaho-saiku_8.0/pentaho) version 8.0 with aliases:
`` `
flexberry / pentaho = flexberry / pentaho: 8.0 = flexberry / pentaho-saiku = flexberry / pentaho-saiku: 8.0
`` `

At the moment, the development of a tree of images 8.1.
Its features are:
- image flexberry / pentaho: 8.1 contains the minimum set of functionality included in the core of Pentaho 8.1 CE
- image of flexberry / pentaho-saiku: 8.1 s a child of the image of flexberry / pentaho: 8.1. When assembling it, the projects source codes
[/Flexberry/saiku€ (https://github.com/Flexberry/saiku),
[Flexberry / fop] (https://github.com/Flexberry/fop)
 will be used.


The image tree will look like this:

`` `
flexberry / pentaho = flexberry / pentaho: 8.0 == flexberry / pentaho-saiku = flexberry / pentaho-saiku: 8.0
                  |
                  + flexberry / pentaho: 8.1 -> flexberry / pentaho-saiku: 8.1
`` `

As soon as the aliases of flexberry / pentaho, flexberry / pentaho-saiku are ready, they will be transferred from branch 8.0 to branch 8.1
and subsequent versions of images.
