"""Build BossRelics.pck: relic icons and localization."""
import hashlib
import io
import struct
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
from pck_tool import write_pck  # noqa: E402
from PIL import Image  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "build" / "BossRelics.pck"

FORMAT_RGBA8 = 5
DATA_FORMAT_WEBP = 2
ICONS = {
    "images/relics/slippery_sticky_substance.png": ROOT / "assets" / "slippery_sticky_substance.png",
    "images/relics/broken_ritual_horn.png": ROOT / "assets" / "broken_ritual_horn.png",
    "images/relics/kin_war_drum.png": ROOT / "assets" / "kin_war_drum.png",
    "images/relics/sleeping_carapace.png": ROOT / "assets" / "sleeping_carapace.png",
    "images/relics/overpressure_core.png": ROOT / "assets" / "overpressure_core.png",
    "images/relics/ghostly_swim_bladder.png": ROOT / "assets" / "ghostly_swim_bladder.png",
    "images/relics/insatiable_stomach.png": ROOT / "assets" / "insatiable_stomach.png",
    "images/relics/forbidden_tome.png": ROOT / "assets" / "forbidden_tome.png",
    "images/relics/kaiser_twin_claws.png": ROOT / "assets" / "kaiser_twin_claws.png",
}


def make_ctex(webp_bytes: bytes, width: int, height: int) -> bytes:
    header = b"GST2"
    header += struct.pack("<I", 1)
    header += struct.pack("<II", width, height)
    header += struct.pack("<I", 0)
    header += struct.pack("<I", 0)
    header += struct.pack("<III", 0, 0, 0)
    header += struct.pack("<I", DATA_FORMAT_WEBP)
    header += struct.pack("<HH", width, height)
    header += struct.pack("<I", 0)
    header += struct.pack("<I", FORMAT_RGBA8)
    header += struct.pack("<I", len(webp_bytes))
    return header + webp_bytes


def imported_name(res_path: str) -> str:
    digest = hashlib.md5(res_path.encode("utf-8")).hexdigest()
    stem = res_path.rsplit("/", 1)[-1]
    return f"{stem}-{digest}.ctex"


def texture_entries(res_path: str, image_path: Path) -> dict[str, bytes]:
    image = Image.open(image_path)
    if image.size != (256, 256):
        raise SystemExit(f"{image_path} must be 256x256, got {image.size}")
    if image.mode != "RGBA":
        image = image.convert("RGBA")

    buffer = io.BytesIO()
    image.save(buffer, format="WEBP", lossless=True)
    ctex_name = imported_name(res_path)
    import_text = f'''[remap]

importer="texture"
type="CompressedTexture2D"
path="res://.godot/imported/{ctex_name}"

[deps]

source_file="res://{res_path}"
dest_files=["res://.godot/imported/{ctex_name}"]

[params]
'''
    return {
        res_path + ".import": import_text.encode("utf-8"),
        ".godot/imported/" + ctex_name: make_ctex(buffer.getvalue(), *image.size),
    }


def localization_entries() -> dict[str, bytes]:
    entries: dict[str, bytes] = {}
    for language in ("eng", "zhs"):
        for file in sorted((ROOT / "localization" / language).glob("*.json")):
            entries[f"BossRelics/localization/{language}/{file.name}"] = file.read_bytes()
    return entries


def build() -> Path:
    files: dict[str, bytes] = {}
    for resource_path, image_path in ICONS.items():
        files.update(texture_entries(resource_path, image_path))
    files.update(localization_entries())
    OUT.parent.mkdir(parents=True, exist_ok=True)
    write_pck(OUT, files, engine=(4, 5, 1))
    print(f"wrote {OUT} ({OUT.stat().st_size} bytes, {len(files)} files)")
    return OUT


if __name__ == "__main__":
    build()
