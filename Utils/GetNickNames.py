#! /usr/bin/env python3
# -*- coding: utf-8 -*-

import sqlite3


# 函数：根据文件夹名称获取NickName
def get_nicknames_from_savedir_list(savedir_list_file, multi_search_db, micro_msg_db, output_file):
    # 读取文件夹名称
    with open(savedir_list_file, 'r') as file:
        savedir_list = file.read().splitlines()

    # 连接数据库
    conn_multi_search = sqlite3.connect(multi_search_db)
    conn_micro_msg = sqlite3.connect(micro_msg_db)
    cur_multi_search = conn_multi_search.cursor()
    cur_micro_msg = conn_micro_msg.cursor()

    nicknames = []

    for idx, savedir in enumerate(savedir_list):
        # 使用格式化字符串构建查询
        cur_multi_search.execute(f"SELECT entityId FROM SessionAttachInfo WHERE attachPath LIKE '%{savedir}%'")
        entityId_result = cur_multi_search.fetchone()

        if entityId_result:
            entityId = entityId_result[0]

            # 使用entityId查询NameTold表获取userName
            cur_multi_search.execute(f"SELECT userName FROM NameToId WHERE ROWID = {entityId}")
            userName_result = cur_multi_search.fetchone()

            if userName_result:
                userName = userName_result[0]

                # 使用userName查询Contact表获取NickName
                cur_micro_msg.execute(f"SELECT NickName FROM Contact WHERE UserName = '{userName}'")
                nickName_result = cur_micro_msg.fetchone()

                if nickName_result and nickName_result[0]!="":
                    nicknames.append(nickName_result[0])
                else:
                    nicknames.append("Unknown")
        else:
            nicknames.append("Unknown")
        # print(f'quest {idx}: {savedir}, result: entityId {entityId}, nick {nicknames[-1]}')
    # 输出NickName到文件
    # print(f'output to {output_file}, total {len(nicknames)}')
    with open(output_file, 'w', encoding='utf-8') as file:
        for idx, nickname in enumerate(nicknames):
            file.write(nickname)
            if idx != len(nicknames) - 1:
                file.write('\n')

    # 关闭数据库连接
    conn_multi_search.close()
    conn_micro_msg.close()

if __name__ == '__main__':
    # 调用函数
    get_nicknames_from_savedir_list('SaveDirList.txt',
                                    'WCPCache/MultiSearchChatMsg.db.dec.db',
                                    'WCPCache/MicroMsg.db.dec.db',
                                    'ContactNickNameList.txt')