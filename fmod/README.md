# Pipeline de áudio — Capybara vs Cats

## Fonte e publicação

- Projeto FMOD Studio: `fmod/capy-vs-catAudio/capy-vs-catAudio.fspro`
- Versão compatível: FMOD Studio / Unity Integration 2.03.14
- Saída do FMOD Studio: `fmod/capy-vs-catAudio/Build/<plataforma>`
- Destino usado pela build Unity: `Assets/StreamingAssets`
- O vínculo está configurado em `Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset`.

Depois de editar áudio no FMOD Studio, use **Build > Build All Platforms**. Ao retornar ao Unity, use **FMOD > Refresh Banks** e confirme que não há eventos ausentes.

## Organização do mixer

- `bus:/Music` / `vca:/MUS`: músicas.
- `bus:/SFX` / `vca:/SFX`: gameplay, armas, motor e mortes.
- `bus:/UI` / `vca:/UI`: interface.
- `bus:/Reverb`: retornos de ambiente.
- `snapshot:/Pause`: tratamento sonoro de Game Over e conclusão de fase.

O slider **SOM** controla o master. O slider **SFX** controla os VCAs SFX e UI. O slider **DUB** controla as vozes Unity legadas da seleção de personagem.

Os samples do banco Master são pré-carregados na inicialização. Isso desloca a leitura de disco para a entrada do jogo e evita travamento no primeiro tiro, música ou efeito.

## Eventos conectados

- Menu: `Mus_Theme`; seleção/mapa/lobby/Coming Soon: `Mus_System`.
- Stage 1: `Mus_Gameplay01`; boss: `Mus_Boss01`.
- Jogador: tiro, motor contínuo e morte.
- Inimigos: tiro e morte; Gatoball também reproduz morte no impacto.
- Boss: metralhadora por rajada e morte.
- UI: hover, select, click, start e undo, escolhidos automaticamente pelo tipo/nome do botão.

## Próximos eventos recomendados

O banco atual ainda não contém áudio específico para pickup, escudo, cura, upgrade, dano recebido, laser ou ultimate. Crie eventos próprios nessas categorias antes de conectá-los; não reutilize sons sem relação apenas para preencher silêncio.

Para bullet hell, configure cooldown/polyphony e stealing nos instrumentos do FMOD Studio. O código também limita disparos de áudio simultâneos sem limitar os projéteis visuais ou o dano.
