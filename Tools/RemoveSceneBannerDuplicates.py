"""Remove stray BottomBannerAdController GameObjects from Unity scene YAML (runtime creates one via EnsureExists)."""
import re
import sys
from pathlib import Path

TARGET_NAME = "BottomBannerAdController"


def parse_blocks(text: str):
    parts = re.split(r"(?=--- !u!)", text)
    blocks = []
    for part in parts:
        if not part.strip():
            continue
        header = re.match(r"--- !u!(\d+) &(\d+)", part)
        if not header:
            blocks.append({"ids": set(), "text": part, "go_name": None})
            continue
        type_id, block_id = header.group(1), header.group(2)
        blocks.append(
            {
                "type": type_id,
                "id": block_id,
                "ids": {block_id},
                "text": part,
                "go_name": None,
            }
        )
        if type_id == "1" and f"m_Name: {TARGET_NAME}" in part:
            blocks[-1]["go_name"] = TARGET_NAME
            for m in re.finditer(r"component: \{fileID: (\d+)\}", part):
                blocks[-1]["ids"].add(m.group(1))
    return blocks


def remove_banner_blocks(blocks):
    remove_ids = set()
    for b in blocks:
        if b.get("go_name") == TARGET_NAME:
            remove_ids |= b["ids"]
    if not remove_ids:
        return blocks, 0

    kept = []
    removed = 0
    for b in blocks:
        if b["ids"] & remove_ids:
            removed += 1
            continue
        if b.get("id") in remove_ids:
            removed += 1
            continue
        if b.get("type") in ("114", "4") and re.search(
            rf"m_GameObject: {{fileID: (\d+)}}", b["text"]
        ):
            m = re.search(r"m_GameObject: \{fileID: (\d+)\}", b["text"])
            if m and m.group(1) in remove_ids:
                removed += 1
                continue
        kept.append(b)
    return kept, removed


def clean_scene(path: Path) -> int:
    text = path.read_text(encoding="utf-8")
    blocks = parse_blocks(text)
    kept, removed = remove_banner_blocks(blocks)
    if removed == 0:
        return 0
    out = "".join(b["text"] for b in kept)
    path.write_text(out, encoding="utf-8")
    return removed


def main():
    root = Path(__file__).resolve().parents[1]
    scenes = [root / "Assets/Scenes/MainScene.unity", root / "Assets/Scenes/PetScene.unity"]
    for scene in scenes:
        if not scene.exists():
            continue
        n = clean_scene(scene)
        print(f"{scene.name}: removed {n} YAML block(s) for {TARGET_NAME}")


if __name__ == "__main__":
    main()
