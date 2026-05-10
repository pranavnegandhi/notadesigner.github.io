import os, re, sys

ROOT = r'D:\projects\notadesigner.com'
CONTENT = os.path.join(ROOT, 'hugo-site', 'content')
OUT = os.path.join(ROOT, 'tasks', 'phase3', 'expected-urls.txt')

def parse_fm(text):
    m = re.match(r'^---\r?\n(.*?)\r?\n---', text, re.S)
    if not m:
        return None
    body = m.group(1)
    lines = body.split('\n')
    data = {'aliases': [], 'categories': [], 'tags': [], 'slug': None, 'date': None, 'wp_post_id': None}
    mode = None
    for raw in lines:
        ln = raw.rstrip('\r')
        # list item
        m2 = re.match(r'^\s*-\s*(.*)$', ln)
        if m2 and mode in ('aliases', 'categories', 'tags'):
            v = m2.group(1).strip().strip('"').strip("'")
            data[mode].append(v)
            continue
        m3 = re.match(r'^([a-zA-Z_][a-zA-Z0-9_]*):\s*(.*)$', ln)
        if m3:
            key, val = m3.group(1), m3.group(2).strip()
            mode = None
            if key in ('slug','date','wp_post_id'):
                data[key] = val.strip('"').strip("'")
            elif key in ('aliases','categories','tags'):
                if val == '' or val == '[]':
                    if val == '':
                        mode = key
                    # else empty list
                elif val.startswith('[') and val.endswith(']'):
                    inner = val[1:-1]
                    items = [x.strip().strip('"').strip("'") for x in inner.split(',') if x.strip()]
                    data[key] = items
                else:
                    # single inline value
                    data[key] = [val.strip('"').strip("'")]
            # else ignore
        else:
            # blank or non-key line resets mode
            if not ln.strip().startswith('-'):
                mode = None
    return data

records = []
for dirpath, dirs, files in os.walk(CONTENT):
    for fn in files:
        if fn != 'index.md':
            continue
        full = os.path.join(dirpath, fn)
        rel = os.path.relpath(full, ROOT).replace('\\', '/')
        with open(full, 'r', encoding='utf-8') as f:
            text = f.read()
        fm = parse_fm(text)
        if fm is None:
            print('NO FRONTMATTER:', rel)
            continue
        parts = rel.split('/')
        # hugo-site/content/...
        is_post = len(parts) >= 5 and parts[2] == 'posts'
        if is_post:
            dir_name = parts[3]
        else:
            dir_name = parts[2]
        slug = fm['slug'] or dir_name
        url = '/' + slug + '/'
        records.append({
            'rel': rel,
            'is_post': is_post,
            'dir_name': dir_name,
            'slug_field': fm['slug'],
            'final_slug': slug,
            'url': url,
            'wp_post_id': fm['wp_post_id'] or '',
            'date': fm['date'] or '',
            'aliases': fm['aliases'],
            'categories': fm['categories'],
            'tags': fm['tags'],
        })

posts = [r for r in records if r['is_post']]
pages = [r for r in records if not r['is_post']]

# audits
missing_slug = [r['rel'] for r in records if not r['slug_field']]
dir_neq_slug = [(r['rel'], r['dir_name'], r['slug_field']) for r in records if r['slug_field'] and r['slug_field'] != r['dir_name']]

# duplicate alias targets across files: same alias URL appears in multiple files
alias_to_sources = {}
all_aliases = []
for r in records:
    for a in r['aliases']:
        au = a if a.startswith('/') else '/' + a
        if not au.endswith('/'):
            au = au + '/'
        alias_to_sources.setdefault(au, []).append((r['rel'], r['url']))
        all_aliases.append((au, r['url'], r['rel']))

dup_aliases = {k: v for k, v in alias_to_sources.items() if len(v) > 1}

# also check for canonical url collisions
url_to_files = {}
for r in records:
    url_to_files.setdefault(r['url'], []).append(r['rel'])
dup_canonical = {k: v for k, v in url_to_files.items() if len(v) > 1}

posts.sort(key=lambda r: r['url'])
pages.sort(key=lambda r: r['url'])

lines = []
lines.append('# CANONICAL POSTS  (one line per post: <url>\\t<wp_post_id>\\t<source-file>)')
for r in posts:
    lines.append(f"{r['url']}\t{r['wp_post_id']}\t{r['rel']}")
lines.append('# CANONICAL PAGES')
for r in pages:
    lines.append(f"{r['url']}\t{r['wp_post_id']}\t{r['rel']}")
lines.append('# ALIASES  (one line per alias: <alias-url>\\t<canonical-url>\\t<source-file>)')
for au, canon, rel in sorted(all_aliases):
    lines.append(f"{au}\t{canon}\t{rel}")
lines.append('# AUDIT NOTES')
lines.append(f"- total posts: {len(posts)}")
lines.append(f"- total pages: {len(pages)}")
lines.append(f"- total aliases: {len(all_aliases)}")
if missing_slug:
    lines.append(f"- posts missing slug: {', '.join(missing_slug)}")
else:
    lines.append('- posts missing slug: none')
if dup_aliases:
    lines.append('- duplicate alias targets across files:')
    for k, v in sorted(dup_aliases.items()):
        lines.append(f"    {k} -> {v}")
else:
    lines.append('- duplicate alias targets across files: none')
if dup_canonical:
    lines.append('- duplicate canonical URLs:')
    for k, v in sorted(dup_canonical.items()):
        lines.append(f"    {k} -> {v}")
else:
    lines.append('- duplicate canonical URLs: none')
if dir_neq_slug:
    lines.append('- posts whose dir-name != slug:')
    for rel, d, s in dir_neq_slug:
        lines.append(f"    {rel}: dir={d} slug={s}")
else:
    lines.append('- posts whose dir-name != slug: none')

os.makedirs(os.path.dirname(OUT), exist_ok=True)
with open(OUT, 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines) + '\n')

print(f"Wrote {OUT}")
print(f"posts={len(posts)} pages={len(pages)} aliases={len(all_aliases)}")
