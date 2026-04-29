# Desktop UI (Electron)

## Geliştirme / Çalıştırma

Makinede `node` / `npm` yoksa bile, `./dev.sh` portable Node indirip `npm install` ve `npm start` adımlarını başlatmaya çalışır.

1. `cd desktop-ui`
2. `./dev.sh`

İstersen Node sürümünü değiştirebilirsin:

```sh
NODE_VERSION=20.11.1 ./dev.sh
```

## Notlar

- CI akışı zaten `.github/workflows/release.yml` üzerinden node kurup build eder.
- Windows v1 için paketleme/dağıtım ayrı pipeline ile yapılır.
