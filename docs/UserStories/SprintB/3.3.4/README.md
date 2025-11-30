# US 3.3.4 - Visual Styling with Textures

## Descrição

As a System User, I want 3D models to be rendered with appropriate textures or visual styling, so that different port elements (e.g., docks, vessels, storage areas, cranes) are easily distinguishable.

## Critérios de Aceitação

- Each category of 3D object (e.g., vessels, docks, storage areas, cranes) must have distinct textures and materials.
- Regarding procedurally created models, texture and material properties and locations must be retrieved from the backend as JSON-formatted content. Additionally, textures must include at least two maps: a color map and either a roughness map, a bump map, or a normal map.
- Textures or materials must not significantly degrade performance or loading time.
