# Maintainer: PatzminiHD <7.0@gmx.at>
pkgname=vav1comp-git
_pkgname=${pkgname%-git}
pkgver=1.4.0
pkgrel=1
pkgdesc="Re-encode videos to AV1"
arch=('x86_64')
url_vav1comp="https://github.com/PatzminiHD/vav1comp"
url_lib="https://github.com/PatzminiHD/PatzminiHD.CSLib"
license=('GPL3')
depends=(
    "icu"
    "zlib"
    "ffmpeg"
)
makedepends=(
    "git"
    "dotnet-host"
    "dotnet-sdk"
)
options=("staticlibs" "!strip")
source=(
    "git+${url_vav1comp}.git"
    "git+${url_lib}.git"
)
sha512sums=("SKIP" "SKIP")

build() {
  cd "vav1comp"

  MSBUILDDISABLENODEREUSE=1 dotnet publish --self-contained --runtime linux-x64 --output ../$pkgname.tmp
}

package() {
  install -d $pkgdir/opt/
  install -d $pkgdir/usr/bin/

  cp -r $pkgname.tmp "$pkgdir/opt/$pkgname/"
  rm "$pkgdir/opt/$pkgname/VideoAV1Compressor.pdb"
  ln -s "/opt/$pkgname/VideoAV1Compressor" "$pkgdir/usr/bin/$_pkgname"
}
