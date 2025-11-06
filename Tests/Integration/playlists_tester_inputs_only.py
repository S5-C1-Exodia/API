#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Integration test: GET /api/spotify/playlists (US 1.2)
- Uses an existing SessionId obtained after completing the OAuth flow.
- Fetches first page, prints a compact summary, then follows NextPageToken once (optional).
- Repeats the first page call to demonstrate DB cache (second call should be faster / identical).
"""

import sys
import json
import time
import requests


def prompt(msg, default=None):
    try:
        val = input(msg).strip()
        if not val and default is not None:
            return default
        return val
    except EOFError:
        return default


def pretty_item(i, idx):
    pid = i.get("playlistId") or i.get("PlaylistId")
    name = i.get("name") or i.get("Name")
    tracks = i.get("trackCount") or i.get("TrackCount")
    selected = i.get("selected") or i.get("Selected")
    return f"[{idx:02d}] {pid or '-'} | {name or '-'} | tracks={tracks if tracks is not None else '-'} | selected={bool(selected)}"


def call_playlists(base_url, sid, page_token=None, verify_tls=False, timeout=30):
    headers = {"X-Session-Id": sid}
    if page_token:
        headers["X-Page-Token"] = page_token
    url = f"{base_url.rstrip('/')}/api/spotify/playlists"
    t0 = time.perf_counter()
    resp = requests.get(url, headers=headers, timeout=timeout, verify=verify_tls)
    dt = (time.perf_counter() - t0) * 1000.0
    return resp, dt


def main():
    print("===========================================================")
    print("🎧  INTEGRATION TEST — SwipeZ API — /api/spotify/playlists")
    print("===========================================================\n")

    # Interactive inputs only (no environment variables)
    default_base = "https://df4501fd7f3a.ngrok-free.app"
    base_url = prompt(f"Base URL [{default_base}]: ", default_base)
    verify_ans = prompt("Verify TLS? (y/N) [N]: ", "n").lower()
    verify_tls = verify_ans in ("y", "yes", "1", "true")

    sid = prompt("Enter X-Session-Id (from OAuth flow): ").strip()
    if not sid:
        print("❌ Missing SessionId. Aborting.")
        sys.exit(2)

    print("\nStep 1️⃣  — Fetch FIRST PAGE (no X-Page-Token)")
    resp1, dt1 = call_playlists(base_url, sid, page_token=None, verify_tls=verify_tls)
    if resp1.status_code >= 400:
        print(f"❌ GET /playlists FAILED: HTTP {resp1.status_code} payload: {resp1.text}")
        if resp1.status_code == 401:
            print("   → Check that your SessionId is valid and linked to a TokenSet.")
        sys.exit(1)

    data1 = resp1.json()
    items1 = data1.get("items") or data1.get("Items") or []
    next_token = data1.get("nextPageToken") or data1.get("NextPageToken")

    print(f"✅ First page OK in {dt1:.1f} ms — {len(items1)} item(s)")
    for idx, it in enumerate(items1, start=1):
        print("   " + pretty_item(it, idx))
    print(f"   NextPageToken = {next_token!r}")

    print("\nStep 2️⃣  — Repeat FIRST PAGE (expect cache hit)")
    resp2, dt2 = call_playlists(base_url, sid, page_token=None, verify_tls=verify_tls)
    if resp2.status_code >= 400:
        print(f"❌ Second GET /playlists FAILED: HTTP {resp2.status_code} payload: {resp2.text}")
        sys.exit(1)

    data2 = resp2.json()
    items2 = data2.get("items") or data2.get("Items") or []
    next_token_2 = data2.get("nextPageToken") or data2.get("NextPageToken")

    # Simple equality check (structure-wise)
    eq = json.dumps(data1, sort_keys=True) == json.dumps(data2, sort_keys=True)
    print(f"✅ Second page OK in {dt2:.1f} ms — cache {'MATCH' if eq else 'DIFF'}")
    if not eq:
        print("   (Response differs; TTL may have expired or server mapping changed.)")
    print(f"   NextPageToken = {next_token_2!r}")

    if next_token:
        print("\nStep 3️⃣  — Fetch NEXT PAGE using NextPageToken")
        resp3, dt3 = call_playlists(base_url, sid, page_token=next_token, verify_tls=verify_tls)
        if resp3.status_code >= 400:
            print(f"❌ GET /playlists (next) FAILED: HTTP {resp3.status_code} payload: {resp3.text}")
            sys.exit(1)

        data3 = resp3.json()
        items3 = data3.get("items") or data3.get("Items") or []
        next_token_3 = data3.get("nextPageToken") or data3.get("NextPageToken")

        print(f"✅ Next page OK in {dt3:.1f} ms — {len(items3)} item(s)")
        for idx, it in enumerate(items3, start=1):
            print("   " + pretty_item(it, idx))
        print(f"   NextPageToken = {next_token_3!r}")
    else:
        print("\nℹ️  No NextPageToken — nothing more to fetch.")

    print("\n💾 DB expectations (after first non-cached call):")
    print("  - playlistcache: one row for (ProviderUserId, PageToken=NULL) with Json/UpdatedAt/ExpiresAt")
    print("  - playlistcache_session: link for (SessionId, ProviderUserId, PageToken=NULL) with LinkedAt")
    print("\n✅ Integration test finished.")


if __name__ == "__main__":
    main()
