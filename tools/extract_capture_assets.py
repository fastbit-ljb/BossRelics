"""Extract a boss's original Spine resources into the local capture project."""
from __future__ import annotations

import json
import re
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / "tools" / "capture_project"
GAME = Path(r"D:\L\Game\Steam\steamapps\common\Slay the Spire 2")
PCK = GAME / "SlayTheSpire2.pck"
sys.path.insert(0, str(ROOT / "tools"))
from pck_tool import extract, read_pck  # noqa: E402


def put(info: dict, resource_path: str) -> bytes:
    resource_path = resource_path.removeprefix("res://")
    data = extract(info, resource_path)
    target = PROJECT / resource_path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(data)
    return data


def imported_paths(import_text: str) -> list[str]:
    return re.findall(r'res://([^"\]]+)', import_text)


def put_import(info: dict, source_path: str) -> bytes:
    source_path = source_path.removeprefix("res://")
    text = put(info, source_path + ".import").decode("utf-8")
    for dest in imported_paths(text):
        if dest in info["files"]:
            put(info, dest)
    return text.encode("utf-8")


def extract_boss(scene_path: str, output_name: str) -> dict:
    info = read_pck(PCK)

    # Load the Spine extension used by the shipped game.
    put(info, "addons/spine/spine_godot_extension.gdextension")
    dll_target = PROJECT / "addons" / "spine" / "windows" / GAME.joinpath(
        "libspine_godot.windows.template_release.x86_64.dll"
    ).name
    dll_target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(GAME / dll_target.name, dll_target)

    scene_data = extract(info, scene_path).decode("utf-8")
    skeleton_match = re.search(
        r'\[ext_resource type="SpineSkeletonDataResource"[^\n]+path="res://([^"]+)"',
        scene_data,
    )
    if not skeleton_match:
        raise RuntimeError(f"No SpineSkeletonDataResource in {scene_path}")
    skeleton_data_path = skeleton_match.group(1)
    skeleton_data_text = put(info, skeleton_data_path).decode("utf-8")

    source_paths = re.findall(
        r'\[ext_resource type="(?:SpineAtlasResource|SpineSkeletonFileResource)"[^\n]+path="res://([^"]+)"',
        skeleton_data_text,
    )
    for source_path in source_paths:
        put_import(info, source_path)

    # Read texture page names from every extracted Spine atlas and extract their
    # original game texture imports as well.
    for source_path in [p for p in source_paths if p.endswith(".atlas")]:
        import_text = (PROJECT / (source_path + ".import")).read_text("utf-8")
        for dest in imported_paths(import_text):
            if not dest.endswith(".spatlas"):
                continue
            atlas_blob = (PROJECT / dest).read_bytes()
            for page in set(re.findall(rb'([^\\/\n"]+\.png)\\nsize:', atlas_blob)):
                page_name = page.decode("utf-8")
                texture_source = str(Path(source_path).parent / page_name).replace("\\", "/")
                put_import(info, texture_source)

    node_match = re.search(
        r'\[node name="Visuals" type="SpineSprite"[^\]]*\](.*?)(?=\n\[node |\Z)',
        scene_data,
        re.S,
    )
    if not node_match:
        # Some visuals use another node name. Pick the first SpineSprite block.
        node_match = re.search(
            r'\[node [^\n]+type="SpineSprite"[^\]]*\](.*?)(?=\n\[node |\Z)',
            scene_data,
            re.S,
        )
    block = node_match.group(1) if node_match else ""
    animation = re.search(r'preview_animation = "([^"]+)"', block)
    scale = re.search(r'scale = Vector2\(([-\d.]+), ([-\d.]+)\)', block)
    position = re.search(r'position = Vector2\(([-\d.]+), ([-\d.]+)\)', block)

    cfg = {
        "skeleton_data": "res://" + skeleton_data_path,
        "animation": animation.group(1) if animation else "idle_loop",
        "scale_x": float(scale.group(1)) if scale else 1.0,
        "scale_y": float(scale.group(2)) if scale else 1.0,
        "offset_x": float(position.group(1)) if position else 0.0,
        "offset_y": float(position.group(2)) if position else 0.0,
        "output": str((ROOT / "references" / output_name).resolve()).replace("\\", "/"),
    }
    (PROJECT / "capture_config.json").write_text(
        json.dumps(cfg, ensure_ascii=False, indent=2), "utf-8"
    )
    print(json.dumps(cfg, ensure_ascii=False))
    return cfg


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit("usage: extract_capture_assets.py SCENE_PATH OUTPUT.png")
    extract_boss(sys.argv[1], sys.argv[2])
