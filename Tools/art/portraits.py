"""Erzeugt die Anime-Portraits als SVG (Original-Designs für Sternenklingen)."""
import os

W = 512

def defs(bg1, bg2, iris_top, iris_bot, skin="#ffe3d3", skin_shade="#f6c4b0"):
    return f'''
<defs>
  <radialGradient id="bg" cx="50%" cy="40%" r="65%">
    <stop offset="0%" stop-color="{bg1}"/><stop offset="100%" stop-color="{bg2}"/>
  </radialGradient>
  <linearGradient id="iris" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0%" stop-color="{iris_top}"/><stop offset="100%" stop-color="{iris_bot}"/>
  </linearGradient>
  <linearGradient id="skin" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0%" stop-color="{skin}"/><stop offset="100%" stop-color="{skin_shade}"/>
  </linearGradient>
  <clipPath id="circle"><circle cx="256" cy="256" r="250"/></clipPath>
</defs>'''

def frame(inner, ring):
    return f'''<g clip-path="url(#circle)"><rect width="512" height="512" fill="url(#bg)"/>{inner}</g>
<circle cx="256" cy="256" r="247" fill="none" stroke="{ring}" stroke-width="8"/>
<circle cx="256" cy="256" r="239" fill="none" stroke="#ffffff" stroke-opacity="0.35" stroke-width="2"/>'''

def sparkles(color="#ffffff", pts=((80,110,10),(430,150,7),(110,400,6),(420,380,9),(390,70,5))):
    out = ""
    for x, y, s in pts:
        out += f'<path d="M{x},{y-s*2} Q{x},{y} {x+s*2},{y} Q{x},{y} {x},{y+s*2} Q{x},{y} {x-s*2},{y} Q{x},{y} {x},{y-s*2}Z" fill="{color}" opacity="0.8"/>'
    return out

def eye(cx, cy, lash="#2a1a22", mirror=False, sleepy=False, angry=False):
    s = -1 if mirror else 1
    lid_y = cy - (14 if sleepy else 24)
    tilt = 6 if angry else 0
    e = f'''
  <ellipse cx="{cx}" cy="{cy+2}" rx="25" ry="29" fill="#ffffff"/>
  <ellipse cx="{cx}" cy="{cy+5}" rx="19" ry="25" fill="url(#iris)"/>
  <ellipse cx="{cx}" cy="{cy+1}" rx="19" ry="11" fill="#000" opacity="0.25"/>
  <ellipse cx="{cx}" cy="{cy+7}" rx="8" ry="12" fill="#1a0d14"/>
  <circle cx="{cx-7*s}" cy="{cy-5}" r="7" fill="#fff"/>
  <circle cx="{cx+7*s}" cy="{cy+16}" r="3.5" fill="#fff" opacity="0.9"/>'''
    if sleepy:
        e += f'<path d="M{cx-30},{cy-14} Q{cx},{cy-6} {cx+30},{cy-14} L{cx+30},{cy-40} L{cx-30},{cy-40}Z" fill="url(#skin)"/>'
    inner_y = lid_y + (tilt if s == 1 else -tilt)
    outer_y = lid_y - (tilt if s == 1 else -tilt)
    x_in, x_out = (cx + 24*s, cx - 30*s)
    e += f'''
  <path d="M{x_out},{outer_y+6} Q{cx},{lid_y-16} {x_in},{inner_y+2}" stroke="{lash}" stroke-width="7" fill="none" stroke-linecap="round"/>
  <path d="M{x_out},{outer_y+6} L{x_out-8*s},{outer_y-2}" stroke="{lash}" stroke-width="5" stroke-linecap="round"/>
  <path d="M{cx-14},{cy+31} Q{cx},{cy+35} {cx+14},{cy+31}" stroke="{lash}" stroke-width="2.5" fill="none" stroke-linecap="round" opacity="0.7"/>'''
    return e

def face(hair_dark, mouth="smile", brows="normal", sleepy=False, lash="#2a1a22", blush=True):
    f = '''
  <path d="M236,330 L236,400 L276,400 L276,330Z" fill="#f2b9a3"/>
  <path d="M178,215 C176,300 212,352 256,374 C300,352 336,300 334,215 Z" fill="url(#skin)"/>
  <path d="M236,368 Q256,380 276,368 L276,385 Q256,392 236,385Z" fill="#e8a892" opacity="0.6"/>'''
    angry = brows == "angry"
    f += eye(215, 268, lash, False, sleepy, angry) + eye(297, 268, lash, True, sleepy, angry)
    if brows == "angry":
        f += f'<path d="M190,226 L238,238" stroke="{hair_dark}" stroke-width="6" stroke-linecap="round"/><path d="M322,226 L274,238" stroke="{hair_dark}" stroke-width="6" stroke-linecap="round"/>'
    elif brows == "soft":
        f += f'<path d="M194,232 Q214,222 236,230" stroke="{hair_dark}" stroke-width="4" fill="none" stroke-linecap="round"/><path d="M318,232 Q298,222 276,230" stroke="{hair_dark}" stroke-width="4" fill="none" stroke-linecap="round"/>'
    else:
        f += f'<path d="M192,228 Q214,218 238,228" stroke="{hair_dark}" stroke-width="5" fill="none" stroke-linecap="round"/><path d="M320,228 Q298,218 274,228" stroke="{hair_dark}" stroke-width="5" fill="none" stroke-linecap="round"/>'
    f += '<path d="M258,300 L254,312 L260,313" stroke="#d99a88" stroke-width="2.5" fill="none" stroke-linecap="round"/>'
    if mouth == "smile":
        f += '<path d="M240,334 Q256,346 272,334" stroke="#8a3a3a" stroke-width="3.5" fill="none" stroke-linecap="round"/>'
    elif mouth == "grin":
        f += '<path d="M236,330 Q256,352 276,330 Z" fill="#9a3440"/><path d="M240,331 L272,331 L270,336 L242,336Z" fill="#fff"/>'
    elif mouth == "smirk":
        f += '<path d="M242,338 Q258,342 274,330" stroke="#5a1a2a" stroke-width="3.5" fill="none" stroke-linecap="round"/>'
    elif mouth == "calm":
        f += '<path d="M246,337 Q256,340 266,337" stroke="#8a3a3a" stroke-width="3" fill="none" stroke-linecap="round"/>'
    if blush:
        f += '<ellipse cx="196" cy="310" rx="17" ry="8" fill="#ff7a8a" opacity="0.35"/><ellipse cx="316" cy="310" rx="17" ry="8" fill="#ff7a8a" opacity="0.35"/>'
        f += '<path d="M186,308 l6,-6 M194,309 l6,-6 M304,308 l6,-6 M312,309 l6,-6" stroke="#ff6070" stroke-width="2" opacity="0.5"/>'
    return f

BANGS = "M150,262 C138,140 200,92 256,92 C312,92 374,140 362,262 L346,236 L332,266 L316,214 L300,256 L282,206 L264,250 L248,204 L230,252 L214,212 L198,258 L184,226 L168,264 Z"
SPIKY = "M148,268 C130,150 196,84 256,84 C326,84 386,146 364,268 L352,232 L340,270 L322,206 L306,254 L290,196 L268,246 L252,190 L236,248 L216,200 L202,256 L184,214 L170,262 L158,236 Z"
SIDE_L = "M152,236 C140,300 150,360 172,404 L188,340 C178,300 176,270 182,236 Z"
SIDE_R = "M360,236 C372,300 362,360 340,404 L324,340 C334,300 336,270 330,236 Z"

def hair_shine():
    return '<path d="M196,140 Q256,118 316,140" stroke="#ffffff" stroke-opacity="0.45" stroke-width="7" fill="none" stroke-linecap="round"/><path d="M214,156 Q256,142 298,156" stroke="#ffffff" stroke-opacity="0.25" stroke-width="4" fill="none" stroke-linecap="round"/>'

def svg(body):
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="512" height="512">{body}</svg>'

def akari():
    h, hd = "#d8343f", "#8e1d2a"
    inner = sparkles("#ffd27a")
    inner += f'<path d="M330,120 C430,110 470,230 440,360 C430,420 400,470 380,512 L350,512 C390,420 410,320 380,220 C370,180 350,150 330,150Z" fill="{hd}"/>'  # Pferdeschwanz
    inner += f'<path d="M336,128 C420,124 452,230 428,350" stroke="{h}" stroke-width="22" fill="none" stroke-linecap="round"/>'
    inner += f'<path d="M156,250 C150,190 170,130 256,118 C342,130 362,190 356,250 L356,330 L156,330Z" fill="{hd}"/>'
    # Rüstung
    inner += '<path d="M110,512 C120,450 170,410 236,398 L276,398 C342,410 392,450 402,512Z" fill="#5a1a24"/>'
    inner += '<path d="M200,404 L256,470 L312,404" stroke="#f2c14e" stroke-width="8" fill="none"/>'
    inner += '<path d="M96,470 C110,420 160,404 200,410 C190,440 170,470 150,512 L96,512Z" fill="#b8a07a" stroke="#f2c14e" stroke-width="5"/>'
    inner += '<path d="M416,470 C402,420 352,404 312,410 C322,440 342,470 362,512 L416,512Z" fill="#b8a07a" stroke="#f2c14e" stroke-width="5"/>'
    inner += '<circle cx="256" cy="440" r="12" fill="#ff5a3a" stroke="#f2c14e" stroke-width="4"/>'
    inner += face(hd, "grin", "angry")
    inner += f'<path d="{SIDE_L}" fill="{h}"/><path d="{SIDE_R}" fill="{h}"/>'
    inner += f'<path d="{SPIKY}" fill="{h}"/>' + hair_shine()
    # Schleife
    inner += '<path d="M340,128 L380,98 L384,150Z" fill="#f2c14e"/><path d="M340,128 L370,170 L326,160Z" fill="#e0a830"/><circle cx="342" cy="132" r="9" fill="#ffd96a"/>'
    return svg(defs("#ff9a6a", "#7a1020", "#ffb02e", "#c2410c") + frame(inner, "#f2c14e"))

def ren():
    h, hd = "#2fa58f", "#17584f"
    inner = sparkles("#b8ffe6")
    inner += f'<path d="M150,250 C146,180 176,120 256,112 C336,120 366,180 362,250 L360,320 L152,320Z" fill="{hd}"/>'
    # Magierkragen und Umhang
    inner += '<path d="M100,512 C110,440 170,400 256,396 C342,400 402,440 412,512Z" fill="#1d2550"/>'
    inner += '<path d="M180,512 L200,400 L256,430 L312,400 L332,512Z" fill="#2c3778"/>'
    inner += '<path d="M168,420 L200,350 L230,410Z M344,420 L312,350 L282,410Z" fill="#1d2550" stroke="#7ee8c6" stroke-width="4"/>'
    inner += '<path d="M244,440 L256,420 L268,440 L256,462Z" fill="#7ee8c6" stroke="#eafff7" stroke-width="3"/>'
    inner += face(hd, "calm", "soft", sleepy=True, lash="#123a36")
    inner += f'<path d="{SIDE_L}" fill="{h}"/><path d="{SIDE_R}" fill="{h}"/>'
    inner += f'<path d="{SPIKY}" fill="{h}" transform="translate(256,0) scale(-1,1) translate(-256,0)"/>' + hair_shine()
    # Strähne & Ohrring
    inner += f'<path d="M262,96 C240,60 300,50 300,30 C320,70 290,90 276,100Z" fill="{h}"/>'
    inner += '<circle cx="176" cy="352" r="6" fill="#7ee8c6"/><path d="M176,358 L170,378 L182,378Z" fill="#7ee8c6"/>'
    return svg(defs("#7fe3c9", "#0d3b4a", "#7dffb0", "#0f9e5e") + frame(inner, "#7ee8c6"))

def yuki():
    h, hd = "#e4def9", "#9d92d4"
    inner = sparkles("#ffffff", ((90,100,9),(420,120,8),(80,330,6),(440,300,7),(360,60,5),(140,60,5)))
    inner += '<circle cx="410" cy="110" r="44" fill="#fff6c8" opacity="0.85"/><circle cx="430" cy="98" r="40" fill="url(#bg)"/>'
    inner += f'<path d="M140,250 C130,170 180,108 256,104 C332,108 382,170 372,250 C392,340 400,440 410,512 L102,512 C112,440 120,340 140,250Z" fill="{hd}"/>'
    inner += f'<path d="M150,250 C142,180 186,118 256,114 C326,118 370,180 362,250 C378,340 384,430 392,512 L120,512 C128,430 134,340 150,250Z" fill="{h}" opacity="0.6"/>'
    # Priesterinnengewand
    inner += '<path d="M130,512 C140,450 180,410 236,400 L276,400 C332,410 372,450 382,512Z" fill="#f8f6ff"/>'
    inner += '<path d="M236,400 L256,512 L276,400" fill="#b9d4ff"/><path d="M200,410 L256,470 L312,410" stroke="#8fb4f0" stroke-width="6" fill="none"/>'
    inner += face(hd, "smile", "soft", lash="#3a2c5a")
    inner += f'<path d="{SIDE_L}" fill="{h}"/><path d="{SIDE_R}" fill="{h}"/>'
    inner += f'<path d="M154,420 C140,380 150,330 160,300 L176,330 C170,370 172,400 180,430Z" fill="{h}"/><path d="M358,420 C372,380 362,330 352,300 L336,330 C342,370 340,400 332,430Z" fill="{h}"/>'
    inner += f'<path d="{BANGS}" fill="{h}"/>' + hair_shine()
    inner += f'<path d="M180,130 Q220,124 230,150" stroke="{hd}" stroke-width="3" fill="none" opacity="0.6"/>'
    # Mond-Haarspange
    inner += '<path d="M168,166 A28,28 0 1,0 196,128 A22,22 0 1,1 168,166Z" fill="#ffd966" stroke="#c99a1e" stroke-width="3"/><circle cx="200" cy="160" r="5" fill="#9fd3ff"/>'
    return svg(defs("#c9c3ff", "#2c2466", "#a6b8ff", "#4b3fc4") + frame(inner, "#ffe28a"))

def noctis():
    h, hd = "#2e1a48", "#140a24"
    inner = sparkles("#c58cff")
    inner += f'<path d="M126,250 C110,160 180,96 256,96 C332,96 402,160 386,250 C410,350 420,450 430,512 L82,512 C92,450 102,350 126,250Z" fill="{hd}"/>'
    # Hörner
    inner += '<path d="M190,130 C150,90 130,40 150,10 C160,60 190,90 214,112Z" fill="#1a1020" stroke="#8e5ad6" stroke-width="4"/>'
    inner += '<path d="M322,130 C362,90 382,40 362,10 C352,60 322,90 298,112Z" fill="#1a1020" stroke="#8e5ad6" stroke-width="4"/>'
    # Gewand mit hohem Kragen
    inner += '<path d="M100,512 C110,440 170,404 256,400 C342,404 402,440 412,512Z" fill="#1c0f2e"/>'
    inner += '<path d="M150,440 L180,330 L236,400Z M362,440 L332,330 L276,400Z" fill="#3a1d5e" stroke="#c58cff" stroke-width="4"/>'
    inner += '<path d="M236,404 L256,450 L276,404" fill="#b0123a"/><circle cx="256" cy="466" r="12" fill="#ff2a5a" stroke="#c58cff" stroke-width="3"/>'
    inner += face(hd, "smirk", "angry", lash="#0a0510", blush=False).replace('fill="url(#skin)"', 'fill="#f3e2ee"')
    inner += f'<path d="{SIDE_L}" fill="{h}"/><path d="{SIDE_R}" fill="{h}"/>'
    inner += f'<path d="{BANGS}" fill="{h}" transform="translate(256,0) scale(-1,1) translate(-256,0)"/>'
    inner += '<path d="M196,140 Q256,118 316,140" stroke="#a875ff" stroke-opacity="0.6" stroke-width="7" fill="none" stroke-linecap="round"/>'
    # Krone
    inner += '<path d="M206,112 L222,78 L240,104 L256,64 L272,104 L290,78 L306,112Z" fill="#c9a2ff" stroke="#f0e0ff" stroke-width="3"/><circle cx="256" cy="98" r="7" fill="#ff2a5a"/>'
    return svg(defs("#7a3cc0", "#0a0214", "#ff5a6a", "#7a0018") + frame(inner, "#a875ff"))

def slime():
    inner = sparkles("#e0f4ff")
    inner += '<ellipse cx="256" cy="450" rx="190" ry="26" fill="#000" opacity="0.25"/>'
    inner += '<path d="M80,440 C70,330 150,180 256,150 C300,140 300,110 330,100 C320,130 330,150 350,170 C430,240 450,360 432,440 Q256,470 80,440Z" fill="#4aa8ff"/>'
    inner += '<path d="M110,420 C110,330 170,220 250,190" stroke="#bfe6ff" stroke-width="16" fill="none" stroke-linecap="round" opacity="0.7"/>'
    inner += '<ellipse cx="200" cy="320" rx="26" ry="36" fill="#13284a"/><ellipse cx="312" cy="320" rx="26" ry="36" fill="#13284a"/>'
    inner += '<circle cx="190" cy="304" r="10" fill="#fff"/><circle cx="302" cy="304" r="10" fill="#fff"/><circle cx="208" cy="336" r="4" fill="#fff"/><circle cx="320" cy="336" r="4" fill="#fff"/>'
    inner += '<path d="M236,372 Q256,392 276,372" stroke="#13284a" stroke-width="6" fill="none" stroke-linecap="round"/>'
    inner += '<ellipse cx="160" cy="370" rx="20" ry="9" fill="#ff8fb0" opacity="0.5"/><ellipse cx="352" cy="370" rx="20" ry="9" fill="#ff8fb0" opacity="0.5"/>'
    inner += '<path d="M120,180 q10,-30 30,-10 q-10,30 -30,10Z" fill="#7fd3ff"/>'
    return svg(defs("#9fe0c0", "#0e4a3a", "#000", "#000") + frame(inner, "#7fd3ff"))

def kobold():
    inner = sparkles("#b98cff")
    inner += '<path d="M90,512 C100,400 160,330 256,330 C352,330 412,400 422,512Z" fill="#241634"/>'
    # Ohren
    inner += '<path d="M150,250 L40,190 L160,300Z" fill="#6f8a5a"/><path d="M362,250 L472,190 L352,300Z" fill="#6f8a5a"/>'
    inner += '<path d="M150,255 L70,205 L158,290Z" fill="#c28a8a" opacity="0.5"/><path d="M362,255 L442,205 L354,290Z" fill="#c28a8a" opacity="0.5"/>'
    inner += '<ellipse cx="256" cy="290" rx="110" ry="100" fill="#7f9c66"/>'
    # Kapuze
    inner += '<path d="M120,330 C100,190 170,90 256,70 C342,90 412,190 392,330 C380,250 340,200 256,196 C172,200 132,250 120,330Z" fill="#2c1a40"/>'
    inner += '<path d="M256,70 C280,40 320,30 340,40 C310,50 290,70 280,92Z" fill="#2c1a40"/>'
    inner += '<path d="M196,262 L236,282 L196,298 Q182,282 196,262Z" fill="#ff3a4a"/><path d="M316,262 L276,282 L316,298 Q330,282 316,262Z" fill="#ff3a4a"/>'
    inner += '<circle cx="200" cy="278" r="4" fill="#fff"/><circle cx="312" cy="278" r="4" fill="#fff"/>'
    inner += '<path d="M206,340 Q256,370 306,340" stroke="#2a1a10" stroke-width="5" fill="none"/><path d="M222,346 L230,362 L238,352Z M290,346 L282,362 L274,352Z" fill="#fff"/>'
    inner += '<path d="M248,300 L256,316 L264,300" stroke="#4a5a3a" stroke-width="4" fill="none"/>'
    return svg(defs("#5a2a7a", "#0a0412", "#000", "#000") + frame(inner, "#b98cff"))

def golem():
    inner = sparkles("#ffe0a0")
    inner += '<path d="M60,512 L90,380 L180,340 L332,340 L422,380 L452,512Z" fill="#6e5a46"/>'
    inner += '<path d="M150,330 L130,170 L190,100 L320,100 L382,170 L362,330 L300,370 L212,370Z" fill="#8b735a"/>'
    inner += '<path d="M150,330 L130,170 L190,100 L210,120 L170,180 L182,320Z" fill="#a38a6e"/>'
    inner += '<path d="M220,150 L240,190 L226,230 M330,260 L300,290 L310,320 M160,400 L200,440 M360,420 L330,470" stroke="#4a3a2c" stroke-width="5" fill="none"/>'
    inner += '<rect x="172" y="220" width="64" height="26" rx="6" fill="#2a2018"/><rect x="276" y="220" width="64" height="26" rx="6" fill="#2a2018"/>'
    inner += '<rect x="184" y="226" width="40" height="14" rx="5" fill="#ffb84a"/><rect x="288" y="226" width="40" height="14" rx="5" fill="#ffb84a"/>'
    inner += '<rect x="214" y="300" width="84" height="16" rx="4" fill="#2a2018"/>'
    inner += '<path d="M244,120 L256,80 L268,120 L256,140Z" fill="#7ad7ff" stroke="#d8f6ff" stroke-width="3"/>'
    inner += '<path d="M100,420 L112,384 L126,420 L112,436Z M396,430 L410,394 L422,432 L408,446Z" fill="#7ad7ff" stroke="#d8f6ff" stroke-width="2"/>'
    return svg(defs("#3a5aa8", "#0a0c24", "#000", "#000") + frame(inner, "#c8a878"))

def fuchs():
    inner = sparkles("#ffd27a")
    # Flammenschweif
    inner += '<path d="M330,512 C420,470 470,380 440,300 C430,350 400,370 380,360 C420,300 400,220 360,200 C370,260 340,300 320,300Z" fill="#ff7a2a"/>'
    inner += '<path d="M350,512 C420,460 440,400 430,350 C410,390 380,400 360,390Z" fill="#ffd25a"/>'
    inner += '<path d="M120,512 C130,420 180,370 256,366 C332,370 382,420 392,512Z" fill="#f0f0f0"/>'
    # Kopf und Ohren
    inner += '<path d="M150,220 L130,70 L230,170Z" fill="#ff8a3a"/><path d="M362,220 L382,70 L282,170Z" fill="#ff8a3a"/>'
    inner += '<path d="M160,200 L148,100 L214,170Z" fill="#fff0e0"/><path d="M352,200 L364,100 L298,170Z" fill="#fff0e0"/>'
    inner += '<path d="M140,240 C140,170 200,140 256,140 C312,140 372,170 372,240 C372,300 320,330 290,370 L256,400 L222,370 C192,330 140,300 140,240Z" fill="#ff8a3a"/>'
    inner += '<path d="M180,300 C210,320 230,350 256,400 C282,350 302,320 332,300 C310,330 290,350 256,356 C222,350 202,330 180,300Z" fill="#fff4e8"/>'
    inner += '<path d="M190,250 Q214,236 238,256 Q214,266 190,250Z" fill="#2a1408"/><path d="M322,250 Q298,236 274,256 Q298,266 322,250Z" fill="#2a1408"/>'
    inner += '<circle cx="218" cy="250" r="4" fill="#ffd25a"/><circle cx="294" cy="250" r="4" fill="#ffd25a"/>'
    inner += '<ellipse cx="256" cy="368" rx="12" ry="9" fill="#2a1408"/>'
    inner += '<path d="M240,180 Q256,160 272,180 Q256,170 240,180Z" fill="#c8102e"/><circle cx="256" cy="190" r="6" fill="#c8102e"/>'
    inner += '<circle cx="110" cy="150" r="18" fill="#7ad7ff" opacity="0.7"/><circle cx="110" cy="150" r="9" fill="#fff"/>'
    return svg(defs("#ff9a8a", "#3a0618", "#000", "#000") + frame(inner, "#ffb84a"))

if __name__ == "__main__":
    out = os.path.join(os.path.dirname(__file__), "svg")
    os.makedirs(out, exist_ok=True)
    for name, fn in [("akari", akari), ("ren", ren), ("yuki", yuki), ("noctis", noctis),
                     ("schleim", slime), ("kobold", kobold), ("golem", golem), ("fuchs", fuchs)]:
        data = fn()
        for i in ("bg", "iris", "skin", "circle"):
            data = data.replace(f'id="{i}"', f'id="{name}-{i}"').replace(f"url(#{i})", f"url(#{name}-{i})")
        open(os.path.join(out, name + ".svg"), "w").write(data)
    print("ok")
