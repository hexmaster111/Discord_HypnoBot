sudo systemctl stop bot_hypno.service
git pull
sudo systemctl start bot_hypno.service
journalctl -u bot_hypno.service -b --no-pager
