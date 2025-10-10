import sys
import requests


def main():
    base_url = "baseUrl"  # ex: "https://api.swipez.io"
    verify_tls = False

    print("===========================================================")
    print("🧹 TEST D’INTÉGRATION LOGOUT SPOTIFY — SWIPEZ API")
    print("===========================================================\n")

    # Récupération du SessionId
    if len(sys.argv) >= 2:
        session_id = sys.argv[1]
    else:
        session_id = input("➡️  Entre le SessionId ").strip()

    if not session_id:
        print("❌ SessionId manquant. Relance le script en le fournissant.")
        sys.exit(1)

    headers = {"X-Session-Id": session_id}

    # Étape 1 : appel logout
    print("\nÉtape 1️⃣  - Appel /api/spotify/logout ...")
    resp = requests.post(
        f"{base_url}/api/spotify/logout",
        headers=headers,
        timeout=20,
        verify=verify_tls
    )

    if resp.status_code == 204:
        print("✅ Logout #1 OK : HTTP 204 No Content")
    else:
        print(f"❌ Logout #1 FAILED: HTTP {resp.status_code} payload: {resp.text}")
        sys.exit(1)

    # Étape 2 : idempotence (on rappelle le même endpoint)
    print("\nÉtape 2️⃣  - Idempotence : rappel /api/spotify/logout ...")
    resp2 = requests.post(
        f"{base_url}/api/spotify/logout",
        headers=headers,
        timeout=20,
        verify=verify_tls
    )

    if resp2.status_code == 204:
        print("✅ Logout #2 (idempotent) OK : HTTP 204 No Content")
    else:
        print(f"⚠️ Logout #2: HTTP {resp2.status_code} payload: {resp2.text}")

    print("\n💾 Attendu en base APRÈS logout :")
    print("  - DENYLISTEDREFRESH : +1 ligne (hash du refresh) avec ExpiresAt ≈ now + 90j")
    print("  - ACCESSTOKEN       : 0 ligne pour SessionId =", session_id)
    print("  - PLAYLISTSELECTION : 0 ligne pour SessionId =", session_id)
    print("  - PLAYLISTCACHE_SESSION : 0 ligne pour SessionId =", session_id)
    print("  - PLAYLISTCACHE     : 0 ligne pour ProviderUserId = <du TokenSet lié> (si purge globale choisie)")
    print("  - USERPROFILECACHE  : 0 ligne pour ProviderUserId = <du TokenSet lié> (si purge activée)")
    print("  - TOKENSET          : 0 ligne avec SessionId =", session_id, "(purge forte)")
    print("  - APPSESSION        : 0 ligne avec SessionId =", session_id, "\n")

    print("👉 Requêtes SQL utiles (exemples) :")
    print("    SELECT * FROM DENYLISTEDREFRESH ORDER BY AddedAt DESC LIMIT 5;")
    print("    SELECT * FROM ACCESSTOKEN WHERE SessionId = '{sid}';".format(sid=session_id))
    print("    SELECT * FROM PLAYLISTSELECTION WHERE SessionId = '{sid}';".format(sid=session_id))
    print("    SELECT * FROM PLAYLISTCACHE_SESSION WHERE SessionId = '{sid}';".format(sid=session_id))
    print("    SELECT * FROM TOKENSET WHERE SessionId = '{sid}';".format(sid=session_id))
    print("    SELECT * FROM APPSESSION WHERE SessionId = '{sid}';".format(sid=session_id))
    print("\n✅ Test d’intégration déconnexion terminé.")


if __name__ == "__main__":
    main()
