import json
import webbrowser
import sys
import requests


def main():
    base_url = "https://822c6d8de2fe.ngrok-free.app"
    verify_tls = False

    scopes = [
        "user-read-email",
        "playlist-read-private",
        "user-library-read"
    ]

    print("===========================================================")
    print("🚀 TEST D’INTÉGRATION AUTH SPOTIFY — SWIPEZ API")
    print("===========================================================\n")

    print("Étape 1️⃣  - Appel /api/spotify/auth/start ...")
    resp = requests.post(
        f"{base_url}/api/spotify/auth/start",
        json={"scopes": scopes},
        timeout=20,
        verify=verify_tls
    )

    if resp.status_code >= 400:
        print(f"❌ StartAuth FAILED: HTTP {resp.status_code} payload: {resp.text}")
        sys.exit(1)

    data = resp.json()
    auth_url = data.get("authorizationUrl") or data.get("authorizationurl") or data.get("AuthorizationUrl")
    state = data.get("state") or data.get("State")

    if not auth_url:
        print("❌ Missing authorizationUrl in response.")
        sys.exit(1)

    print("\n✅ /auth/start OK")
    print("  STATE =", state)
    print("  URL   =", auth_url)

    print("\n💾 Attendu en base APRÈS /auth/start :")
    print("  - Table PKCEENTRY : +1 ligne avec State =", state)
    print("  - Tables APPSESSION et TOKENSET : toujours VIDES (aucune session créée pour l’instant)\n")

    print("Ouverture du navigateur pour authentification Spotify...")
    try:
        webbrowser.open(auth_url, new=1, autoraise=True)
    except Exception as e:
        print("⚠️ Impossible d’ouvrir le navigateur automatiquement:", e)
        print("👉 Ouvre manuellement :", auth_url)

    print("\n👉 Termine le login/consent dans le navigateur.")
    print("   Spotify redirigera ton navigateur vers /api/spotify/callback?code=...&state=...")
    print("   L’API va alors :")
    print("     - Vérifier et consommer le PKCEENTRY (ligne supprimée)")
    print("     - Échanger le code ↔ tokens Spotify")
    print("     - Créer APPSESSION")
    print("     - Créer TOKENSET lié à la session")
    print("     - Rediriger 302 vers le deeplink swipez://...\n")

    input("✅ Appuie sur Entrée UNE FOIS que tu as fini le login dans le navigateur...\n")

    print("💾 Attendu en base APRÈS le callback Spotify :")
    print("  - Table PKCEENTRY : la ligne avec State =", state, "doit être SUPPRIMÉE")
    print("  - Table APPSESSION : +1 ligne nouvelle (nouveau SessionId)")
    print("  - Table TOKENSET : +1 ligne nouvelle liée à ce SessionId\n")

    print("👉 Vérifie maintenant dans MySQL :")
    print("    SELECT * FROM PKCEENTRY;")
    print("    SELECT * FROM APPSESSION;")
    print("    SELECT * FROM TOKENSET;")
    print("\nLe flux d’auth est OK si :")
    print("    - PKCEENTRY vide pour le state consommé")
    print("    - APPSESSION contient une nouvelle session")
    print("    - TOKENSET contient le token Spotify attaché à cette session")
    print("\n✅ Test d’intégration terminé.")


if __name__ == "__main__":
    main()
