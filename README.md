# lionturtle
3D Hexagonal Grid Terrain Maker

A C# Library for creating hex-grid-based terrain in a very particular style that I find beautiful.

![image](https://github.com/pmuren/lionturtle/assets/4354850/16135e43-e0ea-4afe-b239-054cb3a313d6)
<sup> An example rendering of a grid generated by this library. Random colors have been added to distinguish hexagons, and hexes taller than 1 height unit have been painted black. </sup>

![image](https://github.com/pmuren/lionturtle/assets/4354850/4729e2f5-c439-4c99-8c79-9070d9fc5f0b)
<sup> The same grid viewed from above. </sup>

With this library, users can approximate any heightmap that can be sampled at each vertex.
`(x, y) => z`.

I am finally satisfied with the geometry, and now there is so much more I want to do! :)

![image](https://github.com/pmuren/lionturtle/assets/4354850/202778ca-a2bf-4c2d-ab37-b62514cf276e)
<sup> An example Godot 4 project that uses this library to inform the geometry of a mesh. </sup>
