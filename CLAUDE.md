# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a collection of programming articles written in Markdown. The articles explore CS fundamentals (algorithms, data structures, leetcode-style problems) and web development, aimed at senior/staff+ engineers who may have gaps in formal CS education.

The author is a self-taught programmer with 25 years of experience and no formal CS background. The project is both a learning exercise and a teaching resource — solving problems, then writing about the journey.

## Article Specifications

- **Format**: Markdown (.md) files in a flat directory
- **Naming**: `YYYY-MM-DD-slug-name.md` (e.g. `2026-04-08-two-pointer-technique.md`)
- **Length**: 1500–2500 words
- **Code**: C# snippets, moderate density — illustrative, not exhaustive
- **Visuals**: ASCII/text diagrams only when they genuinely clarify what prose and code cannot
- **Structure**: Most articles anchored to a specific problem, but some are concept explorations

## Writing Voice

**This is the most important section. Follow it precisely.**

- **Tone**: Rich, vivid, highly engaging. Never dry or clinical.
- **Hooks**: Open with historical origin stories or war stories from real engineering work. Grab attention in the first paragraph.
- **POV**: First person (I/my) for personal stories and reflections. Second person (you) when teaching or challenging the reader.
- **Energy**: Curiosity-driven discovery — "wait, how DOES this work?" — not lecturing. The reader figures it out alongside the author.
- **Inspiration**: Julia Evans (jvns.ca) — genuine curiosity, making hard things approachable without dumbing them down.
- **Anti-pattern**: Do not produce textbook prose, Wikipedia-style summaries, or dry technical documentation. Every section should have momentum.

## Workflow

1. Pranav provides rough notes, solutions, or scattered thoughts on a topic
2. Claude shapes the raw material into a polished, publication-ready article
3. Iterate on voice, structure, and technical accuracy together

## Content Principles

- Assume the reader is technically experienced but may be encountering this specific concept for the first time
- Show the thinking process, not just the answer — wrong turns and "aha" moments are valuable
- Ground abstract concepts in real-world consequences (performance, production incidents, system design)
- Code examples should be clean and idiomatic C#, with brief inline comments only where the logic isn't obvious
