# Team Setup: Talo

Use this to connect your local game to our shared Talo project.

## 1. Get access

1. Go to `https://dashboard.trytalo.com`.
2. Create your own Talo account.
3. Send me the email address you used so I can invite you to the team project.
4. After I invite you, accept the invite and open the correct team project.

## 2. Enable plugin in Godot

1. Open project folder: `flashcard-roguelike`.
2. In Godot: `Project -> Project Settings -> Plugins`.
3. Find `Talo Game Services`.
4. Set it to `Enabled`.

## 3. Create local settings file

Create this file on your machine:

- `flashcard-roguelike/addons/talo/settings.cfg`

Paste this template:

```ini
access_key="PUT_TEAM_DEV_KEY_HERE"
api_url="https://api.trytalo.com"
socket_url="wss://api.trytalo.com"
auto_connect_socket=true
handle_tree_quit=true
cache_player_on_identify=true
debounce_timer_seconds=1.0

[continuity]
enabled=true

[player_auth]
auto_start_session=true
```

Then replace:

- `PUT_TEAM_DEV_KEY_HERE` with the team dev API key.

## 4. Never commit keys

`settings.cfg` is local-only and must never be committed.

- Do not paste keys into code files.
- Do not include keys in PR screenshots.
- If a key is exposed, rotate it in Talo dashboard.

## 5. Quick test

1. Run the game.
2. Answer flashcards (battle and event rooms both count).
3. Exit game normally.
4. In Talo dashboard, verify stats increase:
   - `flashcard_answers_total`
   - `flashcard_answers_correct`
   - `flashcard_answers_incorrect`

## Note

Dashboard updates are not always instant. If stats/events do not appear right away, wait a bit and refresh.
