"""Create framed boss/relic showcase images from user-provided game screenshots.

The boss and relic artwork are never regenerated, repainted, recolored, warped,
or rotated. The script only crops screenshots, scales proportionally, and lays
the two existing images into an ornamental frame.
"""
from __future__ import annotations

from pathlib import Path
from shutil import copy2

from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
SCREENSHOTS = ROOT / "references" / "boss_screenshots"
ASSETS = ROOT / "assets"
OUTPUT = ASSETS / "showcases"
SCREENSHOTS.mkdir(parents=True, exist_ok=True)
OUTPUT.mkdir(parents=True, exist_ok=True)

ATTACHMENTS = {
    "knowledge_demon": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-e53fd832-d150-4ad0-8779-ad3c831bde65.png"),
    "the_insatiable": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-ddd65eb3-c246-44ea-bc70-0ee4d6918da0.png"),
    "kaiser_claw": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-d91cde0b-c37f-4a34-83cb-6efd3e47caae.png"),
    "kaiser_rocket": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-edd4643a-1c8b-42e1-ae39-b84b1b888b27.png"),
    "soul_fysh": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-831db304-686a-4f0a-96ca-a97365b3397c.png"),
    "waterfall_giant": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-db3dc0c7-bdb9-4bb6-8cae-8cf858faab5d.png"),
    "lagavulin_matriarch": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-6ead6a95-172c-4f4a-8e04-5106ecbe299a.png"),
    "vantom": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-6deb4de2-e446-406e-bd5f-21bcc4f05b83.png"),
    "the_kin": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-57999aea-70f4-46f8-ace1-faf58babbff3.png"),
    "ceremonial_beast": Path(r"C:\Users\liujiabo\AppData\Local\Temp\codex-clipboard-e495c09a-4ceb-4b77-b58c-cd9ff96e8d16.png"),
}

for name, source in ATTACHMENTS.items():
    target = SCREENSHOTS / f"{name}.png"
    if source.exists():
        copy2(source, target)


SHOWCASES = [
    {
        "name": "knowledge_demon_forbidden_tome",
        "shots": [("knowledge_demon", (540, 185, 1395, 950))],
        "relic": "forbidden_tome.png",
        "accent": (195, 151, 54),
    },
    {
        "name": "the_insatiable_insatiable_stomach",
        "shots": [("the_insatiable", (560, 205, 1390, 940))],
        "relic": "insatiable_stomach.png",
        "accent": (206, 117, 45),
    },
    {
        "name": "kaiser_crab_kaiser_twin_claws",
        "shots": [
            ("kaiser_claw", (470, 230, 1435, 930)),
            ("kaiser_rocket", (430, 130, 1450, 930)),
        ],
        "relic": "kaiser_twin_claws.png",
        "accent": (133, 92, 165),
    },
    {
        "name": "soul_fysh_ghostly_swim_bladder",
        "shots": [("soul_fysh", (560, 225, 1360, 930))],
        "relic": "ghostly_swim_bladder.png",
        "accent": (62, 161, 157),
    },
    {
        "name": "waterfall_giant_overpressure_core",
        "shots": [("waterfall_giant", (545, 185, 1390, 950))],
        "relic": "overpressure_core.png",
        "accent": (66, 171, 192),
    },
    {
        "name": "lagavulin_matriarch_sleeping_carapace",
        "shots": [("lagavulin_matriarch", (520, 205, 1410, 950))],
        "relic": "sleeping_carapace.png",
        "accent": (210, 139, 55),
    },
    {
        "name": "vantom_slippery_sticky_substance",
        "shots": [("vantom", (585, 220, 1370, 930))],
        "relic": "slippery_sticky_substance.png",
        "accent": (185, 183, 86),
    },
    {
        "name": "the_kin_kin_war_drum",
        "shots": [("the_kin", (570, 205, 1375, 935))],
        "relic": "kin_war_drum.png",
        "accent": (145, 135, 59),
    },
    {
        "name": "ceremonial_beast_broken_ritual_horn",
        "shots": [("ceremonial_beast", (545, 195, 1400, 950))],
        "relic": "broken_ritual_horn.png",
        "accent": (76, 172, 201),
    },
]


def vertical_gradient(size: tuple[int, int], top: tuple[int, int, int], bottom: tuple[int, int, int]) -> Image.Image:
    w, h = size
    image = Image.new("RGB", size)
    draw = ImageDraw.Draw(image)
    for y in range(h):
        t = y / max(1, h - 1)
        color = tuple(round(a * (1 - t) + b * t) for a, b in zip(top, bottom))
        draw.line((0, y, w, y), fill=color)
    return image


def contain(image: Image.Image, box: tuple[int, int]) -> Image.Image:
    result = image.copy()
    result.thumbnail(box, Image.Resampling.LANCZOS)
    return result


def draw_frame(accent: tuple[int, int, int]) -> Image.Image:
    width, height = 1000, 1200
    canvas = vertical_gradient((width, height), (10, 13, 20), (24, 20, 27)).convert("RGBA")
    draw = ImageDraw.Draw(canvas)
    gold = (174, 131, 57, 255)
    accent_rgba = (*accent, 255)

    # Restrained, flat outer frame: one neutral edge and one accent line.
    draw.rounded_rectangle((28, 28, 972, 1172), radius=34, fill=(17, 20, 27, 255), outline=(55, 52, 57, 255), width=10)
    draw.rounded_rectangle((43, 43, 957, 1157), radius=26, outline=gold, width=3)
    draw.rounded_rectangle((52, 52, 948, 1148), radius=22, outline=(*accent, 185), width=2)

    # Simple boss window and relic panel.
    draw.rounded_rectangle((82, 88, 918, 735), radius=22, fill=(7, 9, 14, 255), outline=(72, 70, 74, 255), width=3)
    draw.rounded_rectangle((91, 97, 909, 726), radius=17, outline=(*accent, 150), width=2)
    draw.rounded_rectangle((310, 818, 690, 1110), radius=30, fill=(9, 11, 17, 255), outline=(*accent, 195), width=3)

    # One understated separator instead of ornamental rails and gems.
    draw.line((115, 772, 885, 772), fill=(93, 78, 52, 255), width=2)
    draw.line((392, 772, 608, 772), fill=accent_rgba, width=3)
    return canvas


def add_relic_glow(canvas: Image.Image, accent: tuple[int, int, int]) -> None:
    glow = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(glow)
    draw.ellipse((365, 850, 635, 1120), fill=(*accent, 48))
    glow = glow.filter(ImageFilter.GaussianBlur(48))
    canvas.alpha_composite(glow)


def paste_bosses(canvas: Image.Image, shots: list[tuple[str, tuple[int, int, int, int]]]) -> None:
    panel = (110, 116, 890, 707)
    panel_w = panel[2] - panel[0]
    panel_h = panel[3] - panel[1]
    if len(shots) == 1:
        name, crop_box = shots[0]
        shot = Image.open(SCREENSHOTS / f"{name}.png").convert("RGB").crop(crop_box)
        fitted = contain(shot, (panel_w, panel_h))
        x = panel[0] + (panel_w - fitted.width) // 2
        y = panel[1] + (panel_h - fitted.height) // 2
        canvas.alpha_composite(fitted.convert("RGBA"), (x, y))
        return

    # Kaiser Crab uses its two original in-game components side by side.
    half_w = panel_w // 2 - 8
    for index, (name, crop_box) in enumerate(shots):
        shot = Image.open(SCREENSHOTS / f"{name}.png").convert("RGB").crop(crop_box)
        fitted = contain(shot, (half_w, panel_h))
        base_x = panel[0] + index * (panel_w // 2 + 8)
        x = base_x + (half_w - fitted.width) // 2
        y = panel[1] + (panel_h - fitted.height) // 2
        canvas.alpha_composite(fitted.convert("RGBA"), (x, y))


def make_showcase(spec: dict) -> Path:
    canvas = draw_frame(spec["accent"])
    paste_bosses(canvas, spec["shots"])
    add_relic_glow(canvas, spec["accent"])

    relic = Image.open(ASSETS / spec["relic"]).convert("RGBA")
    relic = contain(relic, (270, 270))
    canvas.alpha_composite(relic, ((1000 - relic.width) // 2, 838 + (270 - relic.height) // 2))

    output_path = OUTPUT / f"{spec['name']}.png"
    canvas.convert("RGB").save(output_path, quality=96)
    return output_path


if __name__ == "__main__":
    paths = [make_showcase(spec) for spec in SHOWCASES]
    # Keep the filename the user already referenced, but replace its old layout
    # with the new framed vertical composition.
    ceremonial = next(path for path in paths if path.name.startswith("ceremonial_beast_"))
    copy2(ceremonial, ASSETS / "ceremonial_beast_broken_ritual_horn_pair.png")
    for path in paths:
        print(path)
