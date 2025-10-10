#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Integration test: Playlist Tracks Retrieval
Flow:
  1) GET /playlists/{playlistId}/tracks (offset=0)  - First page
  2) GET /playlists/{playlistId}/tracks (offset=20) - Second page
  3) GET /playlists/{playlistId}/tracks (no offset) - Default behavior
  4) GET /playlists/{invalid}/tracks - Error handling
"""

import sys
import json
import time
import requests


def prompt(msg, default=None):
    """Get user input with optional default value"""
    try:
        v = input(msg).strip()
        return (default if (not v and default is not None) else v)
    except EOFError:
        return default


def get_json(resp):
    """Safely parse JSON response"""
    try:
        return resp.json()
    except Exception:
        return {"_raw": resp.text}


def http_get(base_url, path, headers, verify, timeout=30):
    """Execute HTTP GET request with timing"""
    url = f"{base_url.rstrip('/')}{path}"
    t0 = time.perf_counter()
    resp = requests.get(url, headers=headers, timeout=timeout, verify=verify)
    dt = (time.perf_counter() - t0) * 1000.0
    return resp, dt


def print_tracks_info(data, label="Tracks"):
    """Display track information from response"""
    playlist_id = data.get("playlistId") or data.get("PlaylistId") or "N/A"
    tracks = data.get("tracks") or data.get("Tracks") or []

    print(f"   Playlist ID: {playlist_id}")
    print(f"   Tracks count: {len(tracks)}")

    if tracks:
        first_track = tracks[0]
        track_name = first_track.get("name") or first_track.get("Name") or "N/A"
        track_artist = first_track.get("artist") or first_track.get("Artist") or "N/A"
        print(f"   First track: {track_name} - {track_artist}")
    else:
        print("   No tracks returned")


def get_playlist_tracks(base_url, playlist_id, session_id, offset, verify):
    """Fetch playlist tracks with specified offset"""
    # Build query string
    query_params = f"X-Session-Id={session_id}"
    if offset is not None:
        query_params += f"&offset={offset}"

    path = f"/playlists/{playlist_id}/tracks?{query_params}"
    headers = {}

    resp, dt = http_get(base_url, path, headers, verify)
    return resp, dt


def main():
    print("=================================================================")
    print("🎯  INTEGRATION TEST — SwipeZ API — Playlist Tracks Retrieval")
    print("=================================================================\n")

    # Configuration
    base_url = prompt("➡️  Base URL of your API [https://localhost:5001]: ", "https://localhost:5001")
    verify_ans = prompt("➡️  Verify TLS? (y/N) [N]: ", "n").lower()
    verify_tls = verify_ans in ("y", "yes", "1", "true")

    session_id = prompt("➡️  Enter X-Session-Id (from OAuth flow): ", "")
    if not session_id:
        print("❌ Missing SessionId. Aborting.")
        sys.exit(2)

    playlist_id = prompt("➡️  Enter Playlist ID [37i9dQZF1DXcBWIGoYBM5M]: ", "37i9dQZF1DXcBWIGoYBM5M")

    print(f"\n📋 Configuration:")
    print(f"   Base URL: {base_url}")
    print(f"   Session ID: {session_id}")
    print(f"   Playlist ID: {playlist_id}")
    print(f"   Verify TLS: {verify_tls}\n")

    # Step 1: Get first page (offset=0)
    print("Step 1️⃣  — GET /playlists/{playlistId}/tracks (offset=0)")
    resp1, dt1 = get_playlist_tracks(base_url, playlist_id, session_id, 0, verify_tls)

    if resp1.status_code >= 400:
        print(f"❌ GET playlist tracks FAILED: HTTP {resp1.status_code}")
        print(f"   Response: {resp1.text}")
        if resp1.status_code == 401:
            print("\n💡 Hint: Make sure your X-Session-Id is valid and not expired.")
        sys.exit(2)

    data1 = get_json(resp1)
    print(f"✅ First page retrieved in {dt1:.1f} ms")
    print_tracks_info(data1, "First page")

    print("\n🔎 DB expectation:")
    print("   - playlistcache: 1 row for tracks page (PlaylistId, offset=0)")
    print("   - playlistcache_session: link created for (SessionId, PlaylistId, offset=0)")

    tracks1 = data1.get("tracks") or data1.get("Tracks") or []
    track_ids_page1 = set()
    for track in tracks1:
        tid = track.get("trackId") or track.get("TrackId")
        if tid:
            track_ids_page1.add(tid)

    # Step 2: Get second page (offset=20) if first page was full
    if len(tracks1) == 20:
        print("\nStep 2️⃣  — GET /playlists/{playlistId}/tracks (offset=20)")
        resp2, dt2 = get_playlist_tracks(base_url, playlist_id, session_id, 20, verify_tls)

        if resp2.status_code >= 400:
            print(f"❌ GET second page FAILED: HTTP {resp2.status_code}")
            print(f"   Response: {resp2.text}")
        else:
            data2 = get_json(resp2)
            print(f"✅ Second page retrieved in {dt2:.1f} ms")
            print_tracks_info(data2, "Second page")

            tracks2 = data2.get("tracks") or data2.get("Tracks") or []

            if tracks2:
                # Check that pages have different tracks
                track_ids_page2 = set()
                for track in tracks2:
                    tid = track.get("trackId") or track.get("TrackId")
                    if tid:
                        track_ids_page2.add(tid)

                overlap = track_ids_page1 & track_ids_page2
                if overlap:
                    print(f"⚠️  WARNING: Found {len(overlap)} overlapping tracks between pages!")
                else:
                    print("   ✓ Pages contain different tracks (as expected)")

            print("\n🔎 DB expectation:")
            print("   - playlistcache: additional row for tracks page (PlaylistId, offset=20)")
    else:
        print(f"\nℹ️  Playlist has only {len(tracks1)} tracks, skipping page 2 test.")

    # Step 3: Get without offset (default behavior)
    print("\nStep 3️⃣  — GET /playlists/{playlistId}/tracks (no offset)")
    resp3, dt3 = get_playlist_tracks(base_url, playlist_id, session_id, None, verify_tls)

    if resp3.status_code >= 400:
        print(f"❌ GET default page FAILED: HTTP {resp3.status_code}")
        print(f"   Response: {resp3.text}")
    else:
        data3 = get_json(resp3)
        print(f"✅ Default page retrieved in {dt3:.1f} ms")
        print_tracks_info(data3, "Default page")

        tracks3 = data3.get("tracks") or data3.get("Tracks") or []
        if len(tracks3) == len(tracks1):
            print("   ✓ Default page matches first page count (as expected)")
        else:
            print(f"   ⚠️  Default page has {len(tracks3)} tracks vs {len(tracks1)} in first page")

    # Step 4: Test error handling with invalid playlist
    print("\nStep 4️⃣  — GET /playlists/{invalid}/tracks (Error handling)")
    invalid_playlist = "invalid-playlist-id-xyz-999"
    resp4, dt4 = get_playlist_tracks(base_url, invalid_playlist, session_id, 0, verify_tls)

    print(f"   Response status: HTTP {resp4.status_code}")

    if resp4.status_code >= 400:
        print(f"   ✓ Expected error received: {resp4.status_code}")
        error_data = get_json(resp4)
        error_msg = error_data.get("error") or error_data.get("message") or resp4.text[:100]
        print(f"   Error message: {error_msg}")
    else:
        print(f"   ⚠️  Expected error but got success: {resp4.status_code}")

    # Step 5: Performance summary
    print("\n📊 Performance Summary:")
    print(f"   First page (offset=0):  {dt1:.1f} ms")
    if len(tracks1) == 20:
        print(f"   Second page (offset=20): {dt2:.1f} ms")
    print(f"   Default page (no offset): {dt3:.1f} ms")
    print(f"   Invalid playlist: {dt4:.1f} ms")

    print("\n✅ Integration test finished successfully.")
    print("\n💡 Next steps:")
    print("   - Verify cache behavior by running the test again (should be faster)")
    print("   - Check database tables: playlistcache, playlistcache_session")
    print("   - Test with different playlists and offsets")


if __name__ == "__main__":
    main()