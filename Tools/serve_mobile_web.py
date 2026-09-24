#!/usr/bin/env python3
"""Serve only an existing mobile WebGL output on a trusted LAN. This does not build Unity."""
import argparse
import functools
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import socket


def validate_build(root: Path) -> None:
    root = root.resolve()
    for name in ('index.html', 'mobile-web.js', 'mobile-web.css'):
        file = root / name
        if not file.is_file() or file.stat().st_size == 0:
            raise ValueError(f'Missing build file: {file}. Run MazeMath > Build > Mobile Web (Experimental).')
    if '{{{' in (root / 'index.html').read_text(encoding='utf-8'):
        raise ValueError('This is a Unity template, not a compiled game. Build it in Unity first.')
    for pattern in ('*.loader.js', '*.wasm*', '*.data*', '*.framework.js*'):
        if not any(p.is_file() and p.stat().st_size for p in (root / 'Build').glob(pattern)):
            raise ValueError(f'Missing compiled output Build/{pattern}; refusing to serve a partial build.')


class MobileWebHandler(SimpleHTTPRequestHandler):
    extensions_map = {**SimpleHTTPRequestHandler.extensions_map,
                      '.wasm': 'application/wasm', '.js': 'application/javascript',
                      '.unityweb': 'application/octet-stream', '.data': 'application/octet-stream'}

    def list_directory(self, path):
        self.send_error(403, 'Directory listing disabled')
        return None

    def send_head(self):
        root = Path(self.directory).resolve()
        target = Path(self.translate_path(self.path)).resolve()
        if target != root and root not in target.parents:
            self.send_error(403, 'Outside build output')
            return None
        relative = target.relative_to(root)
        if any(part.startswith('.') for part in relative.parts):
            self.send_error(403, 'Hidden files are not served')
            return None
        return super().send_head()

    def end_headers(self):
        self.send_header('Cache-Control', 'no-store')  # Local iteration: never mix old index with a new player.
        self.send_header('X-Content-Type-Options', 'nosniff')
        self.send_header('Referrer-Policy', 'same-origin')
        super().end_headers()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--directory', type=Path, default=Path(__file__).resolve().parent.parent / 'Build/MobileWeb')
    parser.add_argument('--host', default='0.0.0.0')
    parser.add_argument('--port', type=int, default=8000)
    args = parser.parse_args()
    try:
        validate_build(args.directory)
    except (ValueError, OSError) as error:
        parser.exit(2, str(error) + '\n')
    handler = functools.partial(MobileWebHandler, directory=str(args.directory.resolve()))
    try:
        server = ThreadingHTTPServer((args.host, args.port), handler)
    except OSError as error:
        parser.exit(2, f'Could not start server: {error}\n')
    try:
        addresses = {addr[4][0] for addr in socket.getaddrinfo(socket.gethostname(), None, socket.AF_INET)}
    except socket.gaierror:
        addresses = set()
    print(f'Serving ONLY {args.directory.resolve()}', flush=True)
    print(f'PC: http://localhost:{args.port}/', flush=True)
    for ip in sorted(addresses):
        if not ip.startswith('127.'):
            print(f'Phone on the same Wi-Fi: http://{ip}:{args.port}/', flush=True)
    if not any(not ip.startswith('127.') for ip in addresses):
        print('Phone: find this PC IPv4 address using ipconfig, then open http://<PC-IP>:8000/ (use your selected port).', flush=True)
    print('Trusted home/private LAN only. Do not forward this port to the internet. Stop: Ctrl+C.', flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
