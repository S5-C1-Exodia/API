#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Integration test: Playlist Preferences (US 1.2)
Flow:
  0) (optional) Fetch /playlists page-1 to auto-pick some playlistIds
  1) PUT    /api/spotify/playlist-preferences        (ReplaceSelection)
  2) GET    /api/spotify/playlist-preferences        (Verify)
  3) PATCH  /api/spotify/playlist-preferences/add    (AddToSelection)
  4) GET    /api/spotify/playlist-preferences        (Verify)
  5) PATCH  /api/spotify/playlist-preferences/remove (RemoveFromSelection)
  6) GET    /api/spotify/playlist-preferences        (Verify)
  7) DELETE /api/spotify/playlist-preferences        (ClearSelection)
  8) GET    /api/spotify/playlist-preferences        (Verify empty)
"""

# !/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Integration test: Playlist Preferences (US 1.2)
"""

import sys
import json
import time
import requests


def prompt(msg, default=None):
    try:
        v = input(msg).strip()
        return (default if (not v and default is not None) else v)
    except EOFError:
        return default


def pretty_ids(ids):
    if not ids:
        return "[]"
    return "[" + ", ".join(ids) + "]"


def get_json(resp):
    try:
        return resp.json()
    except Exception:
        return {"_raw": resp.text}


def http_get(base_url, path, headers, verify, timeout=30):
    url = f"{base_url.rstrip('/')}{path}"
    t0 = time.perf_counter()
    resp = requests.get(url, headers=headers, timeout=timeout, verify=verify)
    dt = (time.perf_counter() - t0) * 1000.0
    return resp, dt


def http_put(base_url, path, headers, body, verify, timeout=30):
    url = f"{base_url.rstrip('/')}{path}"
    t0 = time.perf_counter()
    resp = requests.put(url, headers=headers, json=body, timeout=timeout, verify=verify)
    dt = (time.perf_counter() - t0) * 1000.0
    return resp, dt


def http_patch(base_url, path, headers, body, verify, timeout=30):
    url = f"{base_url.rstrip('/')}{path}"
    t0 = time.perf_counter()
    resp = requests.patch(url, headers=headers, json=body, timeout=timeout, verify=verify)
    dt = (time.perf_counter() - t0) * 1000.0
    return resp, dt


def http_delete(base_url, path, headers, verify, timeout=30):
    url = f"{base_url.rstrip('/')}{path}"
    t0 = time.perf_counter()
    resp = requests.delete(url, headers=headers, timeout=timeout, verify=verify)
    dt = (time.perf_counter() - t0) * 1000.0
    return resp, dt


def fetch_first_page_ids(base_url, sid, verify, pick_count):
    headers = {"X-Session-Id": sid}
    resp, dt = http_get(base_url, "/api/spotify/playlists", headers, verify)
    if resp.status_code >= 400:
        print(f"❌ GET /playlists FAILED: HTTP {resp.status_code} payload: {resp.text}")
        print("\n🔎 DB expectation for playlists cache (after first non-cached hit):")
        print("   - playlistcache: 1 row for (ProviderUserId, PageToken='') with Json/UpdatedAt/ExpiresAt")
        print("   - playlistcache_session: 1 row link (SessionId, ProviderUserId, PageToken='') with LinkedAt")
        return []
    data = get_json(resp)
    items = data.get("items") or data.get("Items") or []
    ids = []
    for it in items:
        pid = it.get("playlistId") or it.get("PlaylistId")
        if pid:
            ids.append(pid)
        if len(ids) >= pick_count:
            break
    print(f"ℹ️  Auto-picked {len(ids)} playlistIds from first page.")
    print("\n🔎 DB expectation for playlists cache (after this call):")
    print("   - playlistcache: upserted page for PageToken='' if it was a cache miss")
    print("   - playlistcache_session: link created for (SessionId, ProviderUserId, PageToken='')")
    return ids


def ask_ids(label, help_msg):
    print(f"\n👉 {help_msg}")
    raw = prompt(f"Enter {label} as comma-separated playlistIds (e.g. pl_1,pl_2) or leave empty to skip: ", "")
    if not raw:
        return []
    parts = [p.strip() for p in raw.split(",")]
    return [p for p in parts if p]


def get_prefs(base_url, sid, verify):
    headers = {"X-Session-Id": sid}
    resp, dt = http_get(base_url, "/api/spotify/playlist-preferences", headers, verify)
    if resp.status_code >= 400:
        print(f"❌ GET preferences FAILED: HTTP {resp.status_code} payload: {resp.text}")
        sys.exit(2)
    data = get_json(resp)
    lst = data.get("playlistIds") or data.get("PlaylistIds") or []
    print(f"✅ GET preferences in {dt:.1f} ms: {pretty_ids(lst)}")
    return lst


def main():
    print("=================================================================")
    print("🎯  INTEGRATION TEST — SwipeZ API — Playlist Preferences (US 1.2)")
    print("=================================================================\n")

    base_url = prompt("➡️  Base URL of your API [https://localhost:5001]: ", "https://localhost:5001")
    verify_ans = prompt("➡️  Verify TLS? (y/N) [N]: ", "n").lower()
    verify_tls = verify_ans in ("y", "yes", "1", "true")

    sid = prompt("➡️  Enter X-Session-Id (from OAuth flow — needed to identify your user session): ", "")
    if not sid:
        print("❌ Missing SessionId. Aborting.")
        sys.exit(2)

    auto_pick = prompt("➡️  Auto-pick playlistIds from /playlists first page? (Y/n) [Y]: ", "y").lower() in ("y", "yes",
                                                                                                             "1",
                                                                                                             "true")
    pick_n = 3
    if auto_pick:
        try:
            pick_n = int(prompt("➡️  How many to pick automatically for the REPLACE test? [3]: ", "3"))
        except ValueError:
            pick_n = 3

    replace_ids = fetch_first_page_ids(base_url, sid, verify_tls, pick_n) if auto_pick else ask_ids(
        "REPLACE ids",
        "These playlists will become the **new full selection** after PUT."
    )
    add_ids = ask_ids(
        "ADD ids",
        "These playlists will be **added** to the current selection after REPLACE."
    )
    rm_ids = ask_ids(
        "REMOVE ids",
        "These playlists will be **removed** from the selection in the REMOVE step."
    )

    headers = {"X-Session-Id": sid}

    # 0) Baseline GET
    print("\nStep 0️⃣  — Current preferences")
    _ = get_prefs(base_url, sid, verify_tls)
    print("\n🔎 DB expectation:")
    print("   - playlistselection: current rows for your SessionId (if any)")

    # 1) PUT Replace
    print("\nStep 1️⃣  — PUT /playlist-preferences (Replace)")
    body = {"playlistIds": replace_ids}
    resp, dt = http_put(base_url, "/api/spotify/playlist-preferences", headers, body, verify_tls)
    if resp.status_code not in (200, 204):
        print(f"❌ PUT /playlist-preferences FAILED: HTTP {resp.status_code} payload: {resp.text}")
        sys.exit(2)
    print(f"✅ Replace OK in {dt:.1f} ms — set to {pretty_ids(replace_ids)}")
    after_replace = get_prefs(base_url, sid, verify_tls)
    print("\n🔎 DB expectation (after REPLACE):")
    print("   - playlistselection: ONLY these rows remain for your SessionId:", pretty_ids(replace_ids))

    # 2) PATCH Add
    print("\nStep 2️⃣  — PATCH /playlist-preferences/add (Add)")
    if not add_ids:
        print("ℹ️  No ADD ids provided — skipping this step.")
    else:
        body = {"playlistIds": add_ids}
        resp, dt = http_patch(base_url, "/api/spotify/playlist-preferences/add", headers, body, verify_tls)
        if resp.status_code not in (200, 204):
            print(f"❌ PATCH /playlist-preferences/add FAILED: HTTP {resp.status_code} payload: {resp.text}")
            sys.exit(2)
        print(f"✅ Add OK in {dt:.1f} ms — added {pretty_ids(add_ids)}")
        after_add = get_prefs(base_url, sid, verify_tls)
        print("\n🔎 DB expectation (after ADD):")
        union_set = sorted(set((replace_ids or [])) | set(add_ids))
        print("   - playlistselection: must contain:", pretty_ids(union_set))

    # 3) PATCH Remove
    print("\nStep 3️⃣  — PATCH /playlist-preferences/remove (Remove)")
    if not rm_ids:
        print("ℹ️  No REMOVE ids provided — skipping this step.")
    else:
        body = {"playlistIds": rm_ids}
        resp, dt = http_patch(base_url, "/api/spotify/playlist-preferences/remove", headers, body, verify_tls)
        if resp.status_code not in (200, 204):
            print(f"❌ PATCH /playlist-preferences/remove FAILED: HTTP {resp.status_code} payload: {resp.text}")
            sys.exit(2)
        print(f"✅ Remove OK in {dt:.1f} ms — removed {pretty_ids(rm_ids)}")
        after_rm = get_prefs(base_url, sid, verify_tls)
        print("\n🔎 DB expectation (after REMOVE):")
        print("   - playlistselection: those ids should be gone:", pretty_ids(rm_ids))

    # 4) DELETE Clear
    print("\nStep 4️⃣  — DELETE /playlist-preferences (Clear)")
    resp, dt = http_delete(base_url, "/api/spotify/playlist-preferences", headers, verify_tls)
    if resp.status_code not in (200, 204):
        print(f"❌ DELETE /playlist-preferences FAILED: HTTP {resp.status_code} payload: {resp.text}")
        sys.exit(2)
    print(f"✅ Clear OK in {dt:.1f} ms")
    final_state = get_prefs(base_url, sid, verify_tls)
    print("\n🔎 DB expectation (after CLEAR):")
    print("   - playlistselection: NO rows for your SessionId anymore")

    if final_state:
        print("⚠️ Expected empty preferences after CLEAR, but got:", pretty_ids(final_state))
        sys.exit(1)

    print("\n✅ Integration test finished successfully.")


if __name__ == "__main__":
    main()
