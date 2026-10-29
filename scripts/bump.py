# SPDX-FileCopyrightText: 2026 ddbb07
#
# SPDX-License-Identifier: MIT

# /// script
# requires-python = ">=3.14"
# dependencies = []
# ///

import argparse
import re
import subprocess
from enum import StrEnum
from pathlib import Path

ROOT = Path(__file__).parent.parent
ABOUT = ROOT / "data/About/About.xml"
BRANCH = "main"
VERSION_PATTERN = re.compile(
    r"<modVersion>(?P<major>\d+)\.(?P<minor>\d+)\.(?P<patch>\d+)</modVersion>"
)


class Level(StrEnum):
    PATCH = "patch"
    MINOR = "minor"
    MAJOR = "major"


def bump_version(major: int, minor: int, patch: int, level: Level) -> str:
    if level is Level.MAJOR:
        major, minor, patch = major + 1, 0, 0
    elif level is Level.MINOR:
        minor, patch = minor + 1, 0
    else:
        patch += 1

    return f"{major}.{minor}.{patch}"


def git_output(*arguments: str) -> str:
    return subprocess.run(
        ["git", *arguments], check=True, cwd=ROOT, capture_output=True, text=True
    ).stdout.rstrip()


def check_repository() -> None:
    if git_output("status", "--porcelain"):
        raise SystemExit("Repo is not clean.")

    if (git_output("branch", "--show-current")) != BRANCH:
        raise SystemExit(f"Not on the {BRANCH} branch.")

    git_output("fetch", "--quiet", "origin", BRANCH)
    if int(git_output("rev-list", "--count", f"HEAD..origin/{BRANCH}")):
        raise SystemExit("Local clone is out of date with remote.")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("level", type=Level, choices=list(Level))
    level = parser.parse_args().level

    check_repository()

    text = ABOUT.read_text(encoding="utf-8")
    match = VERSION_PATTERN.search(text)

    if not match:
        parser.error(f"Could not find valid version in: {ABOUT}")

    version = bump_version(
        int(match["major"]),
        int(match["minor"]),
        int(match["patch"]),
        level,
    )

    updated = text[: match.start("major")] + version + text[match.end("patch") :]
    ABOUT.write_text(updated, encoding="utf-8")

    subprocess.run(["git", "add", str(ABOUT)], check=True, cwd=ROOT)
    subprocess.run(
        ["git", "commit", "-m", f"Bump version to {version}", "--", str(ABOUT)],
        check=True,
        cwd=ROOT,
    )


if __name__ == "__main__":
    main()
