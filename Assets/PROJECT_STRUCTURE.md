# Estrutura de assets

- `Scenes`: cenas jogáveis e menus.
- `Scripts`: código organizado por domínio (`Player`, `Enemy`, `Animation`, `Audio`).
- `Prefabs`: prefabs compartilhados entre personagens e fases.
- `UI`: interfaces, telas e arte de menus.
- `naves`: conteúdo específico de cada personagem.
- `inimigos`: arte, animações e prefabs dos inimigos.
- `Componentes do Cenário`: arte e elementos visuais das fases.
- `Editor`: ferramentas internas de configuração e validação.
- `Plugins` e `SREditor`: dependências externas; não misturar com código do jogo.

## Mika

- `naves/Mika/Art`: sprites da nave, projétil e impacto.
- `naves/Mika/Animations`: clips e Animator Controllers.
- `naves/Mika/Prefabs`: nave, projétil e impacto.
- `naves/Mika/Ultimate`: prefab e sprites exclusivos da ultimate.

O prefab editável da ultimate é `naves/Mika/Ultimate/MikaUltimate.prefab`.
