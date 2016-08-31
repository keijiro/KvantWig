Kvant/Wig
=========

*Wig* is a special renderer that simulates and renders hair in a
**non-realistic** fashion.

![gif](http://66.media.tumblr.com/60fd5271645740b3c443bf9d9a9477fd/tumblr_ocmkinFTbU1qio469o1_400.gif)
![gif](http://67.media.tumblr.com/5da8793395bb79f91accd6b94e1a5a59/tumblr_ocmkinFTbU1qio469o2_400.gif)
![gif](http://67.media.tumblr.com/c0847f5efa9d8d3e066e85e5a1c26bc8/tumblr_ocmkinFTbU1qio469o3_400.gif)

Note that this *never* becomes useful for realistic hair simulation. They're
completely different things, and this one is not intended to be realistic!
Please check [HairWorks][HairWorks] or something like that for realistic
simulation.

System Requirements
-------------------

Unity 5.4 or later

How To Apply This To My Own Model?
----------------------------------

Select a mesh inside an imported fbx/obj file, and right click it. From
the context menu, select "Kvant" -> "Wig" -> "Convert to template". This
generates *Wig Template* file from the mesh. Give it to a WigController
component. Then play it. Boom.

Please keep the vertex count low. 500-1,000 would be good. In most cases,
it requires separated low-poly model.

License
-------

Please feel free to use for anything, commercial or noncommercial.

(A proper license text will be added when completed.)

[HairWorks]: https://developer.nvidia.com/hairworks
