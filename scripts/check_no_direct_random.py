import pathlib,re,sys
root = pathlib.Path(__file__).resolve().parents[1]
violations=[]
for scope in ('src','tools','tests'):
    for p in root.joinpath(scope).rglob('*.cs'):
        txt=p.read_text()
        if p.as_posix().endswith('Core/GameRng.cs'):
            continue
        if re.search(r'\bSystem\.Random\b|\bnew\s+Random\s*\(', txt):
            violations.append(str(p.relative_to(root)))
if violations:
    print('violations:', ', '.join(violations))
    sys.exit(1)
print('no_direct_random_ok')
