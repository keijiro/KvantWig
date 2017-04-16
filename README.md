KvantWig
========

**Wig** is a special renderer that simulates and renders hair in a
non-realistic fashion.

![gif](http://i.imgur.com/Mtznb1y.gif)
![gif](http://i.imgur.com/61cZwQ7.gif)

![screenshot](http://i.imgur.com/yIdGoXz.png)
![screenshot](http://i.imgur.com/Mjr9BMy.png)

Note that this is never useful for realistic hair simulation. They're
completely different things, and this one is not intended to be realistic.
It's recommended to check [HairWorks] or similar technologies for realistic
simulation.

[HairWorks]: https://developer.nvidia.com/hairworks

System Requirements
-------------------

Unity 5.5 or later

Installation
------------

Download one of the unitypackage files from the [Releases] page and import it
to a project.

[Releases]: https://github.com/keijiro/KvantWig/releases

How To Apply This To My Own Model?
----------------------------------

Select a mesh inside an imported fbx/obj file and right click it. From the
context menu, select "Kvant" -> "Wig" -> "Convert to template". This generates
**Wig Template** file from the mesh. Give it to a WigController component.
Then play it.

The vertex count of the source model should be kept low because of 64k
vertices limitation in Unity. 500-1,000 would be okay.

License
-------

[MIT](LICENSE.md)
