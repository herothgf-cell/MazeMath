import importlib.util
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location('mobile_server', ROOT / 'Tools/serve_mobile_web.py')
server = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(server)

class MobileServerTests(unittest.TestCase):
    def test_empty_directory_is_not_a_build(self):
        with tempfile.TemporaryDirectory() as d:
            with self.assertRaises(ValueError): server.validate_build(Path(d))

    def test_template_only_is_not_a_build(self):
        with tempfile.TemporaryDirectory() as d:
            p=Path(d); (p/'index.html').write_text('{{{ CODE_FILENAME }}}')
            with self.assertRaises(ValueError): server.validate_build(p)

    def test_complete_output_can_be_served(self):
        with tempfile.TemporaryDirectory() as d:
            p=Path(d); (p/'Build').mkdir()
            for f in ('index.html','mobile-web.css','mobile-web.js','Build/x.loader.js','Build/x.wasm.unityweb','Build/x.data.unityweb','Build/x.framework.js.unityweb'):
                (p/f).write_bytes(b'synthetic test fixture, not a Unity build')
            server.validate_build(p)

    def test_missing_wasm_rejected_even_when_index_exists(self):
        with tempfile.TemporaryDirectory() as d:
            p=Path(d); (p/'Build').mkdir(); (p/'index.html').write_text('not a build')
            with self.assertRaises(ValueError): server.validate_build(p)

    def test_server_has_explicit_wasm_mime(self):
        self.assertEqual(server.MobileWebHandler.extensions_map['.wasm'], 'application/wasm')

if __name__ == '__main__': unittest.main()
