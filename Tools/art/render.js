// Rendert die SVG-Portraits als PNG (512x512) und eine Übersicht.
const { chromium } = require('playwright');
const fs = require('fs'), path = require('path');
(async () => {
  const [svgDir, outDir, gallery] = process.argv.slice(2);
  fs.mkdirSync(outDir, { recursive: true });
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 512, height: 512 } });
  const names = fs.readdirSync(svgDir).filter(f => f.endsWith('.svg')).map(f => f.slice(0, -4));
  for (const n of names) {
    const svg = fs.readFileSync(path.join(svgDir, n + '.svg'), 'utf8');
    await page.setContent(`<html><body style="margin:0;background:transparent">${svg}</body></html>`);
    await page.locator('svg').screenshot({ path: path.join(outDir, n + '.png'), omitBackground: true });
  }
  if (gallery) {
    const order = ['akari','ren','yuki','noctis','schleim','kobold','golem','fuchs'];
    const labels = {akari:'Akari · Flammenklinge',ren:'Ren · Sturmweiser',yuki:'Yuki · Mondpriesterin',noctis:'Noctis · Dunkelfürstin',schleim:'Waldschleim',kobold:'Schattenkobold',golem:'Steingolem',fuchs:'Irrlicht-Fuchs'};
    const cells = order.map(n => `<div class="c"><div class="p">${fs.readFileSync(path.join(svgDir, n + '.svg'),'utf8')}</div><div class="l">${labels[n]}</div></div>`).join('');
    const g = await browser.newPage({ viewport: { width: 1100, height: 640 }, deviceScaleFactor: 2 });
    await g.setContent(`<html><body style="margin:0;background:linear-gradient(#2a1450,#0a0418);font-family:sans-serif">
      <div style="color:#ffd35a;font-size:34px;font-weight:bold;text-align:center;padding:18px 0 4px;letter-spacing:4px">STERNENKLINGEN</div>
      <div style="display:grid;grid-template-columns:repeat(4,1fr);gap:14px;padding:16px 26px">${cells}</div>
      <style>.p svg{width:220px;height:220px}.c{text-align:center}.l{color:#eee;font-size:17px;margin-top:4px}</style></body></html>`);
    await g.screenshot({ path: gallery, fullPage: true });
  }
  await browser.close();
})();
